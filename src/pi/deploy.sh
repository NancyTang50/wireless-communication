#!/bin/bash

pi_ssh_ip="nancy@192.168.178.158"
binary_file="./target/aarch64-unknown-linux-gnu/debug/pi"
output_folder="/home/nancy/"

scp "$binary_file" "$pi_ssh_ip:$output_folder"

ssh $pi_ssh_ip