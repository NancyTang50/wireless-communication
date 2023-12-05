# Running the binary

To run the binary first add the executing rights:
```bash
sudo chmod +x pi
````
Then set the necessary permission for the application:
```bash
sudo setcap 'cap_sys_nice=eip' pi
```
After this the binary can be executing using:
```bash
./pi
```