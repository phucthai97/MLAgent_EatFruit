#!/bin/bash

run_id=$1
file_path="results/$run_id/configuration.yaml"
app_path="Build/2DGround"
time_sleep=1

#max_steps
# Extract the max_steps value from the configuration file and assign it to the current_max_steps variable
current_max_steps=$(grep 'max_steps:' "$file_path" | awk '{print $2}')
interval_max_steps=700000
max_intervals=$2
max_steps_of_flow=$((current_max_steps + max_intervals*interval_max_steps))

#Adjust lamdb
initial_lambd=0
increment_lambd=0.005
max_lambd=0.995

#Check available
folder="results/$1"
if [ -d "$folder" ]; then

  # Extract the value of lambd from the YAML file and assign it to the initial_lambd variable
  initial_lambd=$(grep 'lambd:' "$file_path" | awk '{print $2}')

  echo "Initial max_steps: $current_max_steps"
  
  ###  FIRST RUN!  ####
  #echo -e "\n"
  echo "  ______   _________     _       _______   _________   _________  _______          _       _____  ____  _____  _____  ____  _____   ______   "
  echo ".' ____ \ |  _   _  |   / \     |_   __ \ |  _   _  | |  _   _  ||_   __ \        / \     |_   _||_   \|_   _||_   _||_   \|_   _|.' ___  |  "
  echo "| (___ \_||_/ | | \_|  / _ \      | |__) ||_/ | | \_| |_/ | | \_|  | |__) |      / _ \      | |    |   \ | |    | |    |   \ | | / .'   \_|  "
  echo " _.____\`.     | |     / ___ \     |  __ /     | |         | |      |  __ /      / ___ \     | |    | |\ \| |    | |    | |\ \| | | |   ____  "
  echo "| \____) |   _| |_  _/ /   \ \_  _| |  \ \_  _| |_       _| |_    _| |  \ \_  _/ /   \ \_  _| |_  _| |_\   |_  _| |_  _| |_\   |_\ \`.___]  | "
  echo " \______.'  |_____||____| |____||____| |___||_____|     |_____|  |____| |___||____| |____||_____||_____|\____||_____||_____|\____|\`._____.'  "

  #echo -e "\n"
  
  echo -e "\n|---------------------------------------------|"
  echo "|   Author: PhucThai                          |"
  echo "|   Date create: June 12 2024                 |"
  echo "|   Version: 1.1                              |"
  echo "|   Folder run: results/$1              |"
  echo "|   Interation incremental: $2                 |"
  echo -e "|   Max_step increase of flow: $max_steps_of_flow steps |"
  echo -e "|---------------------------------------------|\n"
  echo -e "\nStart interval - max_steps: $current_max_steps - id=$1 \n"

  # Loop n intervals
  for ((i=1; i<=$max_intervals; i++))
  do
      new_max_steps=$((current_max_steps + i*interval_max_steps))

      ##Replace max_step value
      sed -i "s/    max_steps: [0-9]*/    max_steps: $new_max_steps/g" "$file_path"
      
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
      mlagents-learn $file_path --run-id=$1 --env=$app_path --resume

      if [ $i -ne $max_intervals ]; then
        echo -e "\nSleep for a while!"
        sleep $time_sleep
      fi
  done
else
  echo "Directory $folder not exists. Please check again for run incremental!"
fi
 
