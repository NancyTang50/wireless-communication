// #include "mbed.h"
// #include "ble/BLE.h"


// #define CLIMATE_SERVICE_UUID      "00000001-710e-4a5b-8d75-3e5b444bc3cf"

// const static char DEVICE_NAME[] = "Climate Service";

// class ClimateService : ble::Gap::EventHandler{
//     public:
//     ClimateService(BLE &ble, events::EventQueue &event_queue) :
//         _ble(ble),
//         _event_queue(event_queue),
//         _climate_service_uuid(CLIMATE_SERVICE_UUID),
        
//         _temperature_value(25),  // Initial temperature value
//         _humidity_value(0) // Initial humiditiy value
//     {
//     }

// private:
//     BLE &_ble;
//     events::EventQueue &_event_queue;

//     UUID _climate_service_uuid;
//     uint8_t _temperature_value;
//     uint8_t _humidity_value;

//     ClimateService ClimateService;
// };