#ifndef CHARACTERISTIC_H
#define CHARACTERISTIC_H

#include <cmath>

template<typename BLECharacteristicType, typename NewValueType> class Characteristic {
protected:
    BLECharacteristicType m_characteristic;

    [[nodiscard]] const bool significant_change(float val1, float val2, float threshold) {
        return abs(val1 - val2) >= threshold;
    }
    
public:
    Characteristic(BLECharacteristicType characteristic) : m_characteristic(characteristic) 
    {
    }

    virtual ~Characteristic() {}

    BLECharacteristicType& get_characteristic() {
        return m_characteristic;
    }

    virtual void set_value(NewValueType new_value) = 0; 
};

#endif