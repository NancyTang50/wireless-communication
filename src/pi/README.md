# Running the binary

To run the binary first add the executing rights:
```bash
sudo chmod +x pi
```
Then set the necessary permission for the application:
```bash
sudo setcap 'cap_sys_nice=eip' pi
```
After this the binary can be executing using:
```bash
./pi
```

# Connecting the sensor

The sensor needs to be connected to GPIO pin 4/ Wiring PI pin 7