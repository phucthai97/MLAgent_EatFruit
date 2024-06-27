#!/bin/bash

ori_path="results/resources/configuration.yaml"
file_path="results/configuration.yaml"
app_path="Build/2DGround"
run_id=$1
time_sleep=2

#max_steps
initial_max_steps=400000
max_intervals=$2
max_steps_of_flow=$((initial_max_steps * max_intervals))

#For learning_rate
initial_learningrate=0
decrease_learningrate=0.00003
min_learningrate=0.000005

#For beta
initial_beta=0
decrease_beta=0
min_beta=0.0004

#For epsilon
initial_epsilon=0
decrease_epsilon=0
min_epsilon=0.11

#For lamdb
initial_lambd=0
increment_lambd=0.005
max_lambd=0.999

#Check available
folder="results/$1"
if [ -d "$folder" ]; then
  echo "Directory $folder already exists. Please check again!"
else
  #Copy file .yaml from resource
  cp "$ori_path" "$file_path"

  # Extract the value of lambd from the YAML file and assign it to the initial_lambd variable
  initial_lambd=$(grep 'lambd:' "$file_path" | awk '{print $2}')
  increment_lambd=$(echo "$max_lambd $initial_lambd $max_intervals" | awk '{printf "%.6f", ($1 - $2) / $3}')


  #Extract the value && caculator for decrease paramaters
  initial_learningrate=$(grep 'learning_rate:' "$file_path" | awk '{print $2}')
  decrease_learningrate=$(echo "$initial_learningrate $min_learningrate $max_intervals" | awk '{printf "%.6f", ($1 - $2) / $3}')

  initial_beta=$(grep 'beta:' "$file_path" | awk '{print $2}')
  decrease_beta=$(echo "$initial_beta $min_beta $max_intervals" | awk '{printf "%.6f", ($1 - $2) / $3}')
  

  initial_epsilon=$(grep 'epsilon:' "$file_path" | awk '{print $2}')
  #decrease_epsilon=$(echo "$initial_epsilon $min_epsilon $max_intervals" | awk '{printf "%.6f", ($1 - $2) / $3}')
  decrease_epsilon=$(echo "$initial_epsilon $min_epsilon $max_intervals" | awk '{val = ($1 - $2) / $3; printf "%.6f", int(val * 1000000) / 1000000}')
  echo "initial_lambd $initial_lambd | increment_lambd $increment_lambd"
  echo "initial_learningrate $initial_learningrate | decrease_learningrate $decrease_learningrate"
  echo "initial_beta $initial_beta | decrease_beta $decrease_beta"
  echo "initial_epsilon $initial_epsilon | decrease_epsilon $decrease_epsilon"
  ###


  ###  FIRST RUN!  ####
  #echo -e "\n"
  echo "  ______   _________     _       _______   _________   _________  _______          _       _____  ____  _____  _____  ____  _____   ______   "
  echo ".' ____ \ |  _   _  |   / \     |_   __ \ |  _   _  | |  _   _  ||_   __ \        / \     |_   _||_   \|_   _||_   _||_   \|_   _|.' ___  |  "
  echo "| (___ \_||_/ | | \_|  / _ \      | |__) ||_/ | | \_| |_/ | | \_|  | |__) |      / _ \      | |    |   \ | |    | |    |   \ | | / .'   \_|  "
  echo " _.____\`.     | |     / ___ \     |  __ /     | |         | |      |  __ /      / ___ \     | |    | |\ \| |    | |    | |\ \| | | |   ____  "
  echo "| \____) |   _| |_  _/ /   \ \_  _| |  \ \_  _| |_       _| |_    _| |  \ \_  _/ /   \ \_  _| |_  _| |_\   |_  _| |_  _| |_\   |_\ \`.___]  | "
  echo " \______.'  |_____||____| |____||____| |___||_____|     |_____|  |____| |___||____| |____||_____||_____|\____||_____||_____|\____|\`._____.'  "

  #echo -e "\n"
  
  echo -e "\n|-------------------------------------|"
  echo "|   Author: PhucThai                  |"
  echo "|   Date create: June 11 2024         |"
  echo "|   Version: 1.0                      |"
  echo "|   Folder run: results/$1      |"
  echo "|   Interation: $2                    |"
  echo -e "|   Max_step of flow: $max_steps_of_flow steps   |"
  echo -e "|-------------------------------------|\n"
  echo -e "\nStart interval - max_steps: $initial_max_steps - id=$1 \n"

  sed -i "/    max_steps:/c\    max_steps: ${initial_max_steps}" "$file_path"
  sed -i "/  run_id:/c\  run_id: ${run_id}" "$file_path"

  #Repleace linear -> constant
  sed -i "/      learning_rate_schedule:/c\      learning_rate_schedule: constant" "$file_path"
  sed -i "/      beta_schedule:/c\      beta_schedule: constant" "$file_path"
  sed -i "/      epsilon_schedule:/c\      epsilon_schedule: constant" "$file_path"

  mlagents-learn $file_path --run-id=$run_id --env=$app_path
  echo "Sleep a bit!"
  sleep $time_sleep
  
  # Loop n intervals
  for ((i=1; i<=$max_intervals; i++))
  do
      new_max_steps=$((initial_max_steps + i*initial_max_steps))

      ##Replace max_step value
      sed -i "s/    max_steps: [0-9]*/    max_steps: $new_max_steps/g" "$file_path"
      
      ##After the 4th loop, GAIL and BC mode will be turned off
      if [ $i -eq 4 ]; then
        sed -i 's/strength: 0.8/strength: 1.0/g' "$file_path"
        sed -i -e '37s/^/#/' -e '38s/^/#/' -e '39s/^/#/' "$file_path"
        sed -i -e '40s/^/#/' -e '41s/^/#/' -e '42s/^/#/' "$file_path"
        sed -i -e '51s/^/#/' -e '52s/^/#/' -e '53s/^/#/' "$file_path"
      fi

      if [ $i -ge 1 ]; then
        # Increase lambd value
        new_lambd=$(awk "BEGIN {print $initial_lambd + ($i - 1) * $increment_lambd}")

        # Make sure lambd value does not exceed max_lambd
        if (( $(awk "BEGIN {print ($new_lambd > $max_lambd)}") )); then
          new_lambd=$max_lambd
        fi

        # Replace lambd value in YAML file
        sed -i "s/lambd: [0-9.]\+/lambd: $new_lambd/" "$file_path"
      fi


      #Decrease paramater
        # Increase learning_rate value
        new_learningrate=$(awk "BEGIN {print $initial_learningrate - ($i - 1) * $decrease_learningrate}")
        # Make sure learning_rate value does not exceed learning_rate
        if (( $(awk "BEGIN {print ($new_learningrate < $min_learningrate)}") )); then
          new_learningrate=$min_learningrate
        fi
        # Replace learning_rate value in YAML file
        sed -i "s/learning_rate: [0-9.]\+/learning_rate: $new_learningrate/" "$file_path"

        # Increase beta value
        new_beta=$(awk "BEGIN {print $initial_beta - ($i - 1) * $decrease_beta}")
        # Make sure beta value does not exceed beta
        if (( $(awk "BEGIN {print ($new_beta < $min_beta)}") )); then
          new_beta=$min_beta
        fi
        # Replace beta value in YAML file
        sed -i "s/beta: [0-9.]\+/beta: $new_beta/" "$file_path"

        # Increase epsilon value
        new_epsilon=$(awk "BEGIN {print $initial_epsilon - ($i - 1) * $decrease_epsilon}")
        # Make sure epsilon value does not exceed epsilon
        if (( $(awk "BEGIN {print ($new_epsilon < $min_epsilon)}") )); then
          new_epsilon=$min_epsilon
        fi
        # Replace epsilon value in YAML file
        sed -i "s/epsilon: [0-9.]\+/epsilon: $new_epsilon/" "$file_path"
      ###
      

      echo -e "\nInterval $i - max_steps: $new_max_steps - id=$1 \n"
      mlagents-learn $file_path --run-id=$1 --env=$app_path --resume

      if [ $i -ne $max_intervals ]; then
        echo -e "\nSleep for a while!"
        sleep $time_sleep
      fi
  done
fi
 
