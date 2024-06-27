#!/bin/bash

if [ "$1" == "all" ]; then
    tensorboard --logdir results
else
    tensorboard --logdir results/Player16
fi
