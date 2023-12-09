#include "mbed.h"
#include "ble/BLE.h"

// main() runs in its own thread in the OS
int main()
{
    while (true) {
    BLE &ble = BLE::Instance();
    ble.init();

    // service hub
    
    //
    ble.gattServer().addService(myService)

    }
}

