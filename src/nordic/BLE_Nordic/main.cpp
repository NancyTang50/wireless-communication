#include "mbed.h"
#include "ble/BLE.h"
#include "BLE_Services/environmental_service.h"
#include "ble/BLE.h"
#include <cstdint>

#define DEVICE_NAME     "BLE_Nordic"

static events::EventQueue event_queue(/* event count */ 16 * EVENTS_EVENT_SIZE);

/* Schedule processing of events from the BLE middleware in the event queue. */
void schedule_ble_events(BLE::OnEventsToProcessCallbackContext *context)
{
    event_queue.call(Callback<void()>(&context->ble, &BLE::processEvents));
}

void completeInitialBle(BLE::InitializationCompleteCallbackContext *context){
    if (context->error == BLE_ERROR_NONE) {
        return;
        
    }
    // TODO: show error
}

void startAdvertise(BLE &_ble){

}

// main() runs in its own thread in the OS
int main()
{
    BLE &ble = BLE::Instance();
    ble.init(completeInitialBle);
    EnvironmentalService environmentalService(ble);
    startAdvertise(ble);
    ble.onEventsToProcess(schedule_ble_events);
    event_queue.dispatch_forever();
}