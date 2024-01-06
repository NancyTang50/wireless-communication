# PI Peripheral

To develop the code for the PI, please follow the install guide for [docker](https://www.docker.com/get-started/). This will ensure that the Rust language is correctly install with the cross compilation for the Raspberry PIs architecture.

To compile the code in debug mode run the following command:
```bash
cargo b
```
To compile the code in release mode run the following command:
```bash
cargo b -r
```

Then the binary in the can be found in the folder `.\target\aarch64-unknown-linux-gnu\` and then de debug or release folder.
There you can find the binary named `pi`. This binary needs to be copied to the Raspberry PI.

To run the binary first add the executing rights:
```bash
sudo chmod +x pi
```

Then turn off the timesync service of the pi:
```bash
sudo systemctl stop systemd-timesyncd
```

After this the binary can be executing using:
```bash
sudo ./pi
```

## Wiring diagram

To connect the DHT22 to the Raspberry PI see the wiring diagram:

![Wiring diagram for the PI](./docs/RaspberryPiWiringDiagram.png)
