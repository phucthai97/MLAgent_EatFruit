#!/bin/bash

# Lọc các hàng chứa chuỗi "[INFO] MoveAgent2D. Step:" và lưu vào file đầu ra
grep "\[INFO\] MoveAgent2D\. Step:" "$1" > "$2"

# Thông báo hoàn thành
echo "Log filled -> $2"

