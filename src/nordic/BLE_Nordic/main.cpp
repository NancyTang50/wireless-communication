#include "mbed.h"
#include "ble/BLE.h"
#include "Services/climateService.h"

// main() runs in its own thread in the OS
int main()
{
    BLE &ble = BLE::Instance();
    ble.init();

    // ClimateService
    ClimateService climateService(ble);
}

