# Test code

To get the right encoding for the time, execute this js and send this via bluetooth

```js
const date = new Date();
const yearBytes = new Uint16Array([date.getFullYear()]);

const bleEncodedBytes = [];
bleEncodedBytes.push(yearBytes[0] & 0xFF);
bleEncodedBytes.push((yearBytes[0] >> 8) & 0xFF);
bleEncodedBytes.push(date.getMonth());
bleEncodedBytes.push(date.getDate());
bleEncodedBytes.push(date.getHours());
bleEncodedBytes.push(date.getMinutes());
bleEncodedBytes.push(date.getSeconds());

console.log(`Value is ${Array.from(bleEncodedBytes, byte => byte.toString(16).padStart(2, '0')).join('')}`);
```
