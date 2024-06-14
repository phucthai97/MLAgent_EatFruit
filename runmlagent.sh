#!/bin/bash

ori_path="results/resources/configuration.yaml"
file_path="results/configuration.yaml"
run_id=$1
time_sleep=1

#max_steps
initial_max_steps=300000
max_intervals=$2
max_steps_of_flow=$((initial_max_steps * max_intervals))

#Adjust lamdb
initial_lambd=0
increment_lambd=0.0025
max_lambd=0.995

#Check available
folder="results/$1"
if [ -d "$folder" ]; then
  echo "Directory $folder already exists. Please check again!"
else
  #Copy file .yaml from resource
  cp "$ori_path" "$file_path"

  # Extract the value of lambd from the YAML file and assign it to the initial_lambd variable
  initial_lambd=$(grep 'lambd:' "$file_path" | awk '{print $2}')
  
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
  #mlagents-learn $file_path --run-id=$run_id --env=Build/MLAgentBlock --no-graphics
  mlagents-learn $file_path --run-id=$run_id --env=Build/MLAgentBlock
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

      echo -e "\nInterval $i - max_steps: $new_max_steps - id=$1 \n"
      #mlagents-learn $file_path --run-id=$1 --env=Build/MLAgentBlock --no-graphics --resume
      mlagents-learn $file_path --run-id=$1 --env=Build/MLAgentBlock --resume

      if [ $i -ne $max_intervals ]; then
        echo -e "\nSleep for a while!"
        sleep $time_sleep
      fi
  done
fi
 
