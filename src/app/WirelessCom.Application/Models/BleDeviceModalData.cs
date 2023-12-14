﻿using WirelessCom.Domain.Models;

namespace WirelessCom.Application.Models;

public record BleDeviceModalData(BasicBleDevice Device, IReadOnlyList<BareBleAdvertisement> Advertisements, IReadOnlyList<BasicBleService> Services);