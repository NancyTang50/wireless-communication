# PlatformIO installation guide
To install PlatformIO on Visual Studio code, please follow the quickstart guide of [platformIO](https://docs.platformio.org/en/latest/integration/ide/vscode.html#quick-start)

# Wiring DHT-22 sensor to NRF-52840 Diagram
Please wire the DHT-22 sensor exactly in the Wiring Diagram below:
![Wiring diagram](/assets/Wiring%20dht-22%20sensor%20to%20NRF-52840.png)

# Test code
 This test code is used for debugging the encoding and decode the raw bytes to a readable datetime.
 Execute this Javascript below in the console of your browser. For example Google Chrome. After you receive the encoding, send the encoding with the NRF connect mobile app through the Bluetooth as BYTEARRAY.

```js
function EncodeCurrentDate() {
    const date = new Date();
    const yearBytes = new Uint16Array([date.getFullYear()]);

    const bleEncodedBytes = [];
    bleEncodedBytes.push(yearBytes[0] & 0xFF);
    bleEncodedBytes.push((yearBytes[0] >> 8) & 0xFF);
    bleEncodedBytes.push(date.getMonth() + 1);
    bleEncodedBytes.push(date.getDate());
    bleEncodedBytes.push(date.getHours());
    bleEncodedBytes.push(date.getMinutes());
    bleEncodedBytes.push(date.getSeconds());

    const hexNumber = Array.from(bleEncodedBytes, byte => byte.toString(16).padStart(2, '0')).join('');
    console.log(`Value is ${hexNumber}`);
    return hexNumber;
}
```

```js
function DecodeBLEBytes(inputBytes) {
    const bleEncodedBytes = Array.from(inputBytes.match(/.{1,2}/g).map(x => parseInt(x, 16)));
    const year = (bleEncodedBytes[1] << 8 ) | bleEncodedBytes[0];
    const month = bleEncodedBytes[2];
    const day = bleEncodedBytes[3];
    const hours = bleEncodedBytes[4];
    const minutes = bleEncodedBytes[5];
    const seconds = bleEncodedBytes[6];
    const day_of_week = bleEncodedBytes[7];
    const fractions = bleEncodedBytes[8];
    const adjust_reason = bleEncodedBytes[9];

    console.log('Converted time is',
    `year: ${year} 
    month: ${month} 
    day: ${day} 
    hours: ${hours} 
    minutes: ${minutes} 
    seconds: ${seconds}
    day of week: ${day_of_week} 
    fractions: ${fractions} 
    adjust reason: ${adjust_reason}`);
}
```