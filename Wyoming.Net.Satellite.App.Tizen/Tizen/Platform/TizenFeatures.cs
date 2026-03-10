namespace Wyoming.Net.Satellite.App.Tz.Platform;

public static class TizenFeatures
{
    // Account
    public static readonly TizenFeature ACCOUNT = new("http://tizen.org/feature/account", "bool");
    public static readonly TizenFeature ACCOUNT_SYNC = new("http://tizen.org/feature/account.sync", "bool");

    // Accessibility
    public static readonly TizenFeature ACCESSIBILITY_GRAYSCALE = new("http://tizen.org/feature/accessibility.grayscale", "bool");
    public static readonly TizenFeature ACCESSIBILITY_NEGATIVE = new("http://tizen.org/feature/accessibility.negative", "bool");

    // Application History
    public static readonly TizenFeature APP_HISTORY = new("http://tizen.org/feature/app_history", "bool");

    // Attach Panel
    public static readonly TizenFeature ATTACH_PANEL = new("http://tizen.org/feature/attach_panel", "bool");

    // Battery
    public static readonly TizenFeature BATTERY = new("http://tizen.org/feature/battery", "bool");

    // Camera
    public static readonly TizenFeature CAMERA = new("http://tizen.org/feature/camera", "bool");
    public static readonly TizenFeature CAMERA_BACK = new("http://tizen.org/feature/camera.back", "bool");
    public static readonly TizenFeature CAMERA_BACK_FLASH = new("http://tizen.org/feature/camera.back.flash", "bool");
    public static readonly TizenFeature CAMERA_FRONT = new("http://tizen.org/feature/camera.front", "bool");
    public static readonly TizenFeature CAMERA_FRONT_FLASH = new("http://tizen.org/feature/camera.front.flash", "bool");

    // Consumer IR
    public static readonly TizenFeature CONSUMER_IR = new("http://tizen.org/feature/consumer_ir", "bool");

    // Content
    public static readonly TizenFeature CONTENT_SCANNING_OTHERS = new("http://tizen.org/feature/content.scanning.others", "bool");
    public static readonly TizenFeature CONTENT_FILTER_PINYIN = new("http://tizen.org/feature/content.filter.pinyin", "bool");

    // Contextual Trigger
    public static readonly TizenFeature CONTEXTUAL_TRIGGER = new("http://tizen.org/feature/contextual_trigger", "bool");

    // Database
    public static readonly TizenFeature DATABASE_ENCRYPTION = new("http://tizen.org/feature/database.encryption", "bool");

    // Diagnostics
    public static readonly TizenFeature DIAGNOTICS = new("http://tizen.org/feature/diagnotics", "bool");

    // Download
    public static readonly TizenFeature DOWNLOAD = new("http://tizen.org/feature/download", "bool");

    // Feedback
    public static readonly TizenFeature FEEDBACK_VIBRATION = new("http://tizen.org/feature/feedback.vibration", "bool");

    // FIDO
    public static readonly TizenFeature FIDO_UAF = new("http://tizen.org/feature/fido.uaf", "bool");

    // FM Radio
    public static readonly TizenFeature FMRADIO = new("http://tizen.org/feature/fmradio", "bool");

    // Graphics
    public static readonly TizenFeature GRAPHICS_ACCELERATION = new("http://tizen.org/feature/graphics.acceleration", "bool");

    // Human Activity Monitor
    public static readonly TizenFeature HUMANACTIVITYMONITOR = new("http://tizen.org/feature/humanactivitymonitor", "bool");

    // Input
    public static readonly TizenFeature INPUT_KEYBOARD = new("http://tizen.org/feature/input.keyboard", "bool");
    public static readonly TizenFeature INPUT_KEYBOARD_LAYOUT = new("http://tizen.org/feature/input.keyboard.layout", "String");
    public static readonly TizenFeature INPUT_ROTATING_BEZEL = new("http://tizen.org/feature/input.rotating_bezel", "bool");
    public static readonly TizenFeature INPUT_ROTATING_BEZEL_VIRTUAL = new("http://tizen.org/feature/input.rotating_bezel.virtual", "bool");

    // IoT
    public static readonly TizenFeature IOT_OCF = new("http://tizen.org/feature/iot.ocf", "bool");

    // LED
    public static readonly TizenFeature LED = new("http://tizen.org/feature/led", "bool");

    // Location
    public static readonly TizenFeature LOCATION = new("http://tizen.org/feature/location", "bool");
    public static readonly TizenFeature LOCATION_BATCH = new("http://tizen.org/feature/location.batch", "bool");
    public static readonly TizenFeature LOCATION_GEOFENCE = new("http://tizen.org/feature/location.geofence", "bool");
    public static readonly TizenFeature LOCATION_GPS = new("http://tizen.org/feature/location.gps", "bool");
    public static readonly TizenFeature LOCATION_GPS_SATELLITE = new("http://tizen.org/feature/location.gps.satellite", "bool");
    public static readonly TizenFeature LOCATION_WPS = new("http://tizen.org/feature/location.wps", "bool");

    // Machine Learning
    public static readonly TizenFeature MACHINE_LEARNING_INFERENCE = new("http://tizen.org/feature/machine_learning.inference", "bool");
    public static readonly TizenFeature MACHINE_LEARNING_TRAINING = new("http://tizen.org/feature/machine_learning.training", "bool");

    // Maps
    public static readonly TizenFeature MAPS = new("http://tizen.org/feature/maps", "bool");

    // Media
    public static readonly TizenFeature MEDIA_AUDIO_RECORDING = new("http://tizen.org/feature/media.audio_recording", "bool");
    public static readonly TizenFeature MEDIA_IMAGE_CAPTURE = new("http://tizen.org/feature/media.image_capture", "bool");
    public static readonly TizenFeature MEDIA_VIDEO_RECORDING = new("http://tizen.org/feature/media.video_recording", "bool");

    // Microphone
    public static readonly TizenFeature MICROPHONE = new("http://tizen.org/feature/microphone", "bool");

    // Multimedia
    public static readonly TizenFeature MULTIMEDIA_MEDIA_CODEC = new("http://tizen.org/feature/multimedia.media_codec", "bool");
    public static readonly TizenFeature MULTIMEDIA_PLAYER_SPHERICAL_VIDEO = new("http://tizen.org/feature/multimedia.player.spherical_video", "bool");
    public static readonly TizenFeature MULTIMEDIA_STREAM_RECORDER = new("http://tizen.org/feature/multimedia.stream_recorder", "bool");
    public static readonly TizenFeature MULTIMEDIA_TRANSCODER = new("http://tizen.org/feature/multimedia.transcoder", "bool");

    // Multi-Point Touch
    public static readonly TizenFeature MULTI_POINT_TOUCH_PINCH_ZOOM = new("http://tizen.org/feature/multi_point_touch.pinch_zoom", "bool");
    public static readonly TizenFeature MULTI_POINT_TOUCH_POINT_COUNT = new("http://tizen.org/feature/multi_point_touch.point_count", "int");

    // Network — Bluetooth
    public static readonly TizenFeature NETWORK_BLUETOOTH = new("http://tizen.org/feature/network.bluetooth", "bool");
    public static readonly TizenFeature NETWORK_BLUETOOTH_AUDIO_CALL = new("http://tizen.org/feature/network.bluetooth.audio.call", "bool");
    public static readonly TizenFeature NETWORK_BLUETOOTH_AUDIO_CONTROLLER = new("http://tizen.org/feature/network.bluetooth.audio.controller", "bool");
    public static readonly TizenFeature NETWORK_BLUETOOTH_AUDIO_MEDIA = new("http://tizen.org/feature/network.bluetooth.audio.media", "bool");
    public static readonly TizenFeature NETWORK_BLUETOOTH_HEALTH = new("http://tizen.org/feature/network.bluetooth.health", "bool");
    public static readonly TizenFeature NETWORK_BLUETOOTH_HID = new("http://tizen.org/feature/network.bluetooth.hid", "bool");
    public static readonly TizenFeature NETWORK_BLUETOOTH_HID_DEVICE = new("http://tizen.org/feature/network.bluetooth.hid.device", "bool");
    public static readonly TizenFeature NETWORK_BLUETOOTH_LE = new("http://tizen.org/feature/network.bluetooth.le", "bool");
    public static readonly TizenFeature NETWORK_BLUETOOTH_LE_5_0 = new("http://tizen.org/feature/network.bluetooth.le.5_0", "bool");
    public static readonly TizenFeature NETWORK_BLUETOOTH_LE_GATT_CLIENT = new("http://tizen.org/feature/network.bluetooth.le.gatt.client", "bool");
    public static readonly TizenFeature NETWORK_BLUETOOTH_LE_GATT_SERVER = new("http://tizen.org/feature/network.bluetooth.le.gatt.server", "bool");
    public static readonly TizenFeature NETWORK_BLUETOOTH_LE_IPSP = new("http://tizen.org/feature/network.bluetooth.le.ipsp", "bool");
    public static readonly TizenFeature NETWORK_BLUETOOTH_OOB = new("http://tizen.org/feature/network.bluetooth.oob", "bool");
    public static readonly TizenFeature NETWORK_BLUETOOTH_OPP = new("http://tizen.org/feature/network.bluetooth.opp", "bool");
    public static readonly TizenFeature NETWORK_BLUETOOTH_PHONEBOOK_CLIENT = new("http://tizen.org/feature/network.bluetooth.phonebook.client", "bool");

    // Network — General
    public static readonly TizenFeature NETWORK_ETHERNET = new("http://tizen.org/feature/network.ethernet", "bool");
    public static readonly TizenFeature NETWORK_INM = new("http://tizen.org/feature/network.inm", "bool");
    public static readonly TizenFeature NETWORK_INTERNET = new("http://tizen.org/feature/network.internet", "bool");
    public static readonly TizenFeature NETWORK_MTP = new("http://tizen.org/feature/network.mtp", "bool");
    public static readonly TizenFeature NETWORK_NET_PROXY = new("http://tizen.org/feature/network.net_proxy", "bool");
    public static readonly TizenFeature NETWORK_PUSH = new("http://tizen.org/feature/network.push", "bool");
    public static readonly TizenFeature NETWORK_VPN = new("http://tizen.org/feature/network.vpn", "bool");

    // Network — NFC
    public static readonly TizenFeature NETWORK_NFC = new("http://tizen.org/feature/network.nfc", "bool");
    public static readonly TizenFeature NETWORK_NFC_CARD_EMULATION = new("http://tizen.org/feature/network.nfc.card_emulation", "bool");
    public static readonly TizenFeature NETWORK_NFC_CARD_EMULATION_HCE = new("http://tizen.org/feature/network.nfc.card_emulation.hce", "bool");
    public static readonly TizenFeature NETWORK_NFC_P2P = new("http://tizen.org/feature/network.nfc.p2p", "bool");
    public static readonly TizenFeature NETWORK_NFC_RESERVED_PUSH = new("http://tizen.org/feature/network.nfc.reserved_push", "bool");
    public static readonly TizenFeature NETWORK_NFC_TAG = new("http://tizen.org/feature/network.nfc.tag", "bool");

    // Network — Secure Element
    public static readonly TizenFeature NETWORK_SECURE_ELEMENT = new("http://tizen.org/feature/network.secure_element", "bool");
    public static readonly TizenFeature NETWORK_SECURE_ELEMENT_ESE = new("http://tizen.org/feature/network.secure_element.ese", "bool");
    public static readonly TizenFeature NETWORK_SECURE_ELEMENT_UICC = new("http://tizen.org/feature/network.secure_element.uicc", "bool");

    // Network — Service Discovery
    public static readonly TizenFeature NETWORK_SERVICE_DISCOVERY_DNSSD = new("http://tizen.org/feature/network.service_discovery.dnssd", "bool");
    public static readonly TizenFeature NETWORK_SERVICE_DISCOVERY_SSDP = new("http://tizen.org/feature/network.service_discovery.ssdp", "bool");

    // Network — Telephony
    public static readonly TizenFeature NETWORK_TELEPHONY = new("http://tizen.org/feature/network.telephony", "bool");
    public static readonly TizenFeature NETWORK_TELEPHONY_MMS = new("http://tizen.org/feature/network.telephony.mms", "bool");
    public static readonly TizenFeature NETWORK_TELEPHONY_SERVICE_CDMA = new("http://tizen.org/feature/network.telephony.service.cdma", "bool");
    public static readonly TizenFeature NETWORK_TELEPHONY_SERVICE_EDGE = new("http://tizen.org/feature/network.telephony.service.edge", "bool");
    public static readonly TizenFeature NETWORK_TELEPHONY_SERVICE_GPRS = new("http://tizen.org/feature/network.telephony.service.gprs", "bool");
    public static readonly TizenFeature NETWORK_TELEPHONY_SERVICE_GSM = new("http://tizen.org/feature/network.telephony.service.gsm", "bool");
    public static readonly TizenFeature NETWORK_TELEPHONY_SERVICE_HSDPA = new("http://tizen.org/feature/network.telephony.service.hsdpa", "bool");
    public static readonly TizenFeature NETWORK_TELEPHONY_SERVICE_HSPA = new("http://tizen.org/feature/network.telephony.service.hspa", "bool");
    public static readonly TizenFeature NETWORK_TELEPHONY_SERVICE_HSUPA = new("http://tizen.org/feature/network.telephony.service.hsupa", "bool");
    public static readonly TizenFeature NETWORK_TELEPHONY_SERVICE_LTE = new("http://tizen.org/feature/network.telephony.service.lte", "bool");
    public static readonly TizenFeature NETWORK_TELEPHONY_SERVICE_UMTS = new("http://tizen.org/feature/network.telephony.service.umts", "bool");
    public static readonly TizenFeature NETWORK_TELEPHONY_SMS = new("http://tizen.org/feature/network.telephony.sms", "bool");
    public static readonly TizenFeature NETWORK_TELEPHONY_SMS_CBS = new("http://tizen.org/feature/network.telephony.sms.cbs", "bool");

    // Network — Tethering
    public static readonly TizenFeature NETWORK_TETHERING = new("http://tizen.org/feature/network.tethering", "bool");
    public static readonly TizenFeature NETWORK_TETHERING_BLUETOOTH = new("http://tizen.org/feature/network.tethering.bluetooth", "bool");
    public static readonly TizenFeature NETWORK_TETHERING_USB = new("http://tizen.org/feature/network.tethering.usb", "bool");
    public static readonly TizenFeature NETWORK_TETHERING_WIFI = new("http://tizen.org/feature/network.tethering.wifi", "bool");
    public static readonly TizenFeature NETWORK_TETHERING_WIFI_DIRECT = new("http://tizen.org/feature/network.tethering.wifi.direct", "bool");

    // Network — Wi-Fi
    public static readonly TizenFeature NETWORK_WIFI = new("http://tizen.org/feature/network.wifi", "bool");
    public static readonly TizenFeature NETWORK_WIFI_DIRECT = new("http://tizen.org/feature/network.wifi.direct", "bool");
    public static readonly TizenFeature NETWORK_WIFI_DIRECT_DISPLAY = new("http://tizen.org/feature/network.wifi.direct.display", "bool");
    public static readonly TizenFeature NETWORK_WIFI_DIRECT_SERVICE_DISCOVERY = new("http://tizen.org/feature/network.wifi.direct.service_discovery", "bool");
    public static readonly TizenFeature NETWORK_WIFI_TDLS = new("http://tizen.org/feature/network.wifi.tdls", "bool");

    // OAuth 2.0
    public static readonly TizenFeature OAUTH2 = new("http://tizen.org/feature/oauth2", "bool");

    // OpenGL ES
    public static readonly TizenFeature OPENGLES = new("http://tizen.org/feature/opengles", "bool");
    public static readonly TizenFeature OPENGLES_TEXTURE_FORMAT = new("http://tizen.org/feature/opengles.texture_format", "String");
    public static readonly TizenFeature OPENGLES_TEXTURE_FORMAT_3DC = new("http://tizen.org/feature/opengles.texture_format.3dc", "bool");
    public static readonly TizenFeature OPENGLES_TEXTURE_FORMAT_ATC = new("http://tizen.org/feature/opengles.texture_format.atc", "bool");
    public static readonly TizenFeature OPENGLES_TEXTURE_FORMAT_ETC = new("http://tizen.org/feature/opengles.texture_format.etc", "bool");
    public static readonly TizenFeature OPENGLES_TEXTURE_FORMAT_PTC = new("http://tizen.org/feature/opengles.texture_format.ptc", "bool");
    public static readonly TizenFeature OPENGLES_TEXTURE_FORMAT_PVRTC = new("http://tizen.org/feature/opengles.texture_format.pvrtc", "bool");
    public static readonly TizenFeature OPENGLES_TEXTURE_FORMAT_UTC = new("http://tizen.org/feature/opengles.texture_format.utc", "bool");
    public static readonly TizenFeature OPENGLES_VERSION_1_1 = new("http://tizen.org/feature/opengles.version.1_1", "bool");
    public static readonly TizenFeature OPENGLES_VERSION_2_0 = new("http://tizen.org/feature/opengles.version.2_0", "bool");
    public static readonly TizenFeature OPENGLES_VERSION_3_0 = new("http://tizen.org/feature/opengles.version.3_0", "bool");
    public static readonly TizenFeature OPENGLES_VERSION_3_1 = new("http://tizen.org/feature/opengles.version.3_1", "bool");
    public static readonly TizenFeature OPENGLES_VERSION_3_2 = new("http://tizen.org/feature/opengles.version.3_2", "bool");

    // Peripheral I/O
    public static readonly TizenFeature PERIPHERAL_IO_GPIO = new("http://tizen.org/feature/peripheral_io.gpio", "bool");
    public static readonly TizenFeature PERIPHERAL_IO_I2C = new("http://tizen.org/feature/peripheral_io.i2c", "bool");
    public static readonly TizenFeature PERIPHERAL_IO_PWM = new("http://tizen.org/feature/peripheral_io.pwm", "bool");
    public static readonly TizenFeature PERIPHERAL_IO_ADC = new("http://tizen.org/feature/peripheral_io.adc", "bool");
    public static readonly TizenFeature PERIPHERAL_IO_UART = new("http://tizen.org/feature/peripheral_io.uart", "bool");
    public static readonly TizenFeature PERIPHERAL_IO_SPI = new("http://tizen.org/feature/peripheral_io.spi", "bool");

    // Platform
    public static readonly TizenFeature PLATFORM_CORE_API_VERSION = new("http://tizen.org/feature/platform.core.api.version", "String");
    public static readonly TizenFeature PLATFORM_CORE_ABI = new("http://tizen.org/feature/platform.core.abi", "String");
    public static readonly TizenFeature PLATFORM_CORE_CPU_ARCH = new("http://tizen.org/feature/platform.core.cpu.arch", "String");
    public static readonly TizenFeature PLATFORM_CORE_CPU_ARCH_ARMV6 = new("http://tizen.org/feature/platform.core.cpu.arch.armv6", "bool");
    public static readonly TizenFeature PLATFORM_CORE_CPU_ARCH_ARMV7 = new("http://tizen.org/feature/platform.core.cpu.arch.armv7", "bool");
    public static readonly TizenFeature PLATFORM_CORE_CPU_ARCH_ARMV8 = new("http://tizen.org/feature/platform.core.cpu.arch.armv8", "bool");
    public static readonly TizenFeature PLATFORM_CORE_CPU_ARCH_RISCV32 = new("http://tizen.org/feature/platform.core.cpu.arch.riscv32", "bool");
    public static readonly TizenFeature PLATFORM_CORE_CPU_ARCH_RISCV64 = new("http://tizen.org/feature/platform.core.cpu.arch.riscv64", "bool");
    public static readonly TizenFeature PLATFORM_CORE_CPU_ARCH_X86 = new("http://tizen.org/feature/platform.core.cpu.arch.x86", "bool");
    public static readonly TizenFeature PLATFORM_CORE_CPU_FREQUENCY = new("http://tizen.org/feature/platform.core.cpu.frequency", "int");
    public static readonly TizenFeature PLATFORM_CORE_FPU_ARCH = new("http://tizen.org/feature/platform.core.fpu.arch", "String");
    public static readonly TizenFeature PLATFORM_CORE_FPU_ARCH_SSE2 = new("http://tizen.org/feature/platform.core.fpu.arch.sse2", "bool");
    public static readonly TizenFeature PLATFORM_CORE_FPU_ARCH_SSE3 = new("http://tizen.org/feature/platform.core.fpu.arch.sse3", "bool");
    public static readonly TizenFeature PLATFORM_CORE_FPU_ARCH_SSSE3 = new("http://tizen.org/feature/platform.core.fpu.arch.ssse3", "bool");
    public static readonly TizenFeature PLATFORM_CORE_FPU_ARCH_VFPV2 = new("http://tizen.org/feature/platform.core.fpu.arch.vfpv2", "bool");
    public static readonly TizenFeature PLATFORM_CORE_FPU_ARCH_VFPV3 = new("http://tizen.org/feature/platform.core.fpu.arch.vfpv3", "bool");
    public static readonly TizenFeature PLATFORM_CORE_FPU_ARCH_VFPV4 = new("http://tizen.org/feature/platform.core.fpu.arch.vfpv4", "bool");
    public static readonly TizenFeature PLATFORM_NATIVE_API_VERSION = new("http://tizen.org/feature/platform.native.api.version", "String");
    public static readonly TizenFeature PLATFORM_NATIVE_OSP_COMPATIBLE = new("http://tizen.org/feature/platform.native.osp_compatible", "bool");
    public static readonly TizenFeature PLATFORM_VERSION = new("http://tizen.org/feature/platform.version", "String");
    public static readonly TizenFeature PLATFORM_VERSION_NAME = new("http://tizen.org/feature/platform.version.name", "String");
    public static readonly TizenFeature PLATFORM_WEB_API_VERSION = new("http://tizen.org/feature/platform.web.api.version", "String");

    // Profile
    public static readonly TizenFeature PROFILE = new("http://tizen.org/feature/profile", "String");

    // Screen
    public static readonly TizenFeature SCREEN = new("http://tizen.org/feature/screen", "bool");
    public static readonly TizenFeature SCREEN_ALWAYS_ON_AMOLED = new("http://tizen.org/feature/screen.always_on.amoled", "bool");
    public static readonly TizenFeature SCREEN_ALWAYS_ON_HIGH_COLOR = new("http://tizen.org/feature/screen.always_on.high_color", "bool");
    public static readonly TizenFeature SCREEN_ALWAYS_ON_LOW_BIT_COLOR = new("http://tizen.org/feature/screen.always_on.low_bit_color", "bool");
    public static readonly TizenFeature SCREEN_ALWAYS_ON_VIRTUAL = new("http://tizen.org/feature/screen.always_on.virtual", "bool");
    public static readonly TizenFeature SCREEN_AUTO_ROTATION = new("http://tizen.org/feature/screen.auto_rotation", "bool");
    public static readonly TizenFeature SCREEN_BPP = new("http://tizen.org/feature/screen.bpp", "int");
    public static readonly TizenFeature SCREEN_COORDINATE_SYSTEM_SIZE_LARGE = new("http://tizen.org/feature/screen.coordinate_system.size.large", "bool");
    public static readonly TizenFeature SCREEN_COORDINATE_SYSTEM_SIZE_NORMAL = new("http://tizen.org/feature/screen.coordinate_system.size.normal", "bool");
    public static readonly TizenFeature SCREEN_DPI = new("http://tizen.org/feature/screen.dpi", "int");
    public static readonly TizenFeature SCREEN_HEIGHT = new("http://tizen.org/feature/screen.height", "int");
    public static readonly TizenFeature SCREEN_OUTPUT_HDMI = new("http://tizen.org/feature/screen.output.hdmi", "bool");
    public static readonly TizenFeature SCREEN_OUTPUT_RCA = new("http://tizen.org/feature/screen.output.rca", "bool");
    public static readonly TizenFeature SCREEN_SHAPE_CIRCLE = new("http://tizen.org/feature/screen.shape.circle", "bool");
    public static readonly TizenFeature SCREEN_SHAPE_RECTANGLE = new("http://tizen.org/feature/screen.shape.rectangle", "bool");
    public static readonly TizenFeature SCREEN_SIZE_ALL = new("http://tizen.org/feature/screen.size.all", "bool");
    public static readonly TizenFeature SCREEN_SIZE_LARGE = new("http://tizen.org/feature/screen.size.large", "bool");
    public static readonly TizenFeature SCREEN_SIZE_NORMAL = new("http://tizen.org/feature/screen.size.normal", "bool");
    public static readonly TizenFeature SCREEN_SIZE_NORMAL_240_400 = new("http://tizen.org/feature/screen.size.normal.240.400", "bool");
    public static readonly TizenFeature SCREEN_SIZE_NORMAL_320_320 = new("http://tizen.org/feature/screen.size.normal.320.320", "bool");
    public static readonly TizenFeature SCREEN_SIZE_NORMAL_320_480 = new("http://tizen.org/feature/screen.size.normal.320.480", "bool");
    public static readonly TizenFeature SCREEN_SIZE_NORMAL_360_360 = new("http://tizen.org/feature/screen.size.normal.360.360", "bool");
    public static readonly TizenFeature SCREEN_SIZE_NORMAL_360_480 = new("http://tizen.org/feature/screen.size.normal.360.480", "bool");
    public static readonly TizenFeature SCREEN_SIZE_NORMAL_480_800 = new("http://tizen.org/feature/screen.size.normal.480.800", "bool");
    public static readonly TizenFeature SCREEN_SIZE_NORMAL_540_960 = new("http://tizen.org/feature/screen.size.normal.540.960", "bool");
    public static readonly TizenFeature SCREEN_SIZE_NORMAL_600_1024 = new("http://tizen.org/feature/screen.size.normal.600.1024", "bool");
    public static readonly TizenFeature SCREEN_SIZE_NORMAL_720_1280 = new("http://tizen.org/feature/screen.size.normal.720.1280", "bool");
    public static readonly TizenFeature SCREEN_SIZE_NORMAL_1080_1920 = new("http://tizen.org/feature/screen.size.normal.1080.1920", "bool");
    public static readonly TizenFeature SCREEN_WIDTH = new("http://tizen.org/feature/screen.width", "int");

    // Security
    public static readonly TizenFeature SECURITY_DEVICE_CERTIFICATE = new("http://tizen.org/feature/security.device_certificate", "bool");
    public static readonly TizenFeature SECURITY_PRIVACY_PRIVILEGE = new("http://tizen.org/feature/security.privacy_privilege", "bool");
    public static readonly TizenFeature SECURITY_TEE = new("http://tizen.org/feature/security.tee", "bool");

    // Sensor
    public static readonly TizenFeature SENSOR_ACCELEROMETER = new("http://tizen.org/feature/sensor.accelerometer", "bool");
    public static readonly TizenFeature SENSOR_ACCELEROMETER_WAKEUP = new("http://tizen.org/feature/sensor.accelerometer.wakeup", "bool");
    public static readonly TizenFeature SENSOR_ACTIVITY_RECOGNITION = new("http://tizen.org/feature/sensor.activity_recognition", "bool");
    public static readonly TizenFeature SENSOR_BAROMETER = new("http://tizen.org/feature/sensor.barometer", "bool");
    public static readonly TizenFeature SENSOR_BAROMETER_WAKEUP = new("http://tizen.org/feature/sensor.barometer.wakeup", "bool");
    public static readonly TizenFeature SENSOR_GEOMAGNETIC_ROTATION_VECTOR = new("http://tizen.org/feature/sensor.geomagnetic_rotation_vector", "bool");
    public static readonly TizenFeature SENSOR_GESTURE_RECOGNITION = new("http://tizen.org/feature/sensor.gesture_recognition", "bool");
    public static readonly TizenFeature SENSOR_GRAVITY = new("http://tizen.org/feature/sensor.gravity", "bool");
    public static readonly TizenFeature SENSOR_GYROSCOPE = new("http://tizen.org/feature/sensor.gyroscope", "bool");
    public static readonly TizenFeature SENSOR_GYROSCOPE_ROTATION_VECTOR = new("http://tizen.org/feature/sensor.gyroscope_rotation_vector", "bool");
    public static readonly TizenFeature SENSOR_GYROSCOPE_UNCALIBRATED = new("http://tizen.org/feature/sensor.gyroscope.uncalibrated", "bool");
    public static readonly TizenFeature SENSOR_GYROSCOPE_WAKEUP = new("http://tizen.org/feature/sensor.gyroscope.wakeup", "bool");
    public static readonly TizenFeature SENSOR_HEART_RATE_MONITOR = new("http://tizen.org/feature/sensor.heart_rate_monitor", "bool");
    public static readonly TizenFeature SENSOR_HEART_RATE_MONITOR_BATCH = new("http://tizen.org/feature/sensor.heart_rate_monitor.batch", "bool");
    public static readonly TizenFeature SENSOR_HEART_RATE_MONITOR_LED_GREEN = new("http://tizen.org/feature/sensor.heart_rate_monitor.led_green", "bool");
    public static readonly TizenFeature SENSOR_HEART_RATE_MONITOR_LED_GREEN_BATCH = new("http://tizen.org/feature/sensor.heart_rate_monitor.led_green.batch", "bool");
    public static readonly TizenFeature SENSOR_HEART_RATE_MONITOR_LED_IR = new("http://tizen.org/feature/sensor.heart_rate_monitor.led_ir", "bool");
    public static readonly TizenFeature SENSOR_HEART_RATE_MONITOR_LED_RED = new("http://tizen.org/feature/sensor.heart_rate_monitor.led_red", "bool");
    public static readonly TizenFeature SENSOR_HUMIDITY = new("http://tizen.org/feature/sensor.humidity", "bool");
    public static readonly TizenFeature SENSOR_LINEAR_ACCELERATION = new("http://tizen.org/feature/sensor.linear_acceleration", "bool");
    public static readonly TizenFeature SENSOR_MAGNETOMETER = new("http://tizen.org/feature/sensor.magnetometer", "bool");
    public static readonly TizenFeature SENSOR_MAGNETOMETER_UNCALIBRATED = new("http://tizen.org/feature/sensor.magnetometer.uncalibrated", "bool");
    public static readonly TizenFeature SENSOR_MAGNETOMETER_WAKEUP = new("http://tizen.org/feature/sensor.magnetometer.wakeup", "bool");
    public static readonly TizenFeature SENSOR_PEDOMETER = new("http://tizen.org/feature/sensor.pedometer", "bool");
    public static readonly TizenFeature SENSOR_PHOTOMETER = new("http://tizen.org/feature/sensor.photometer", "bool");
    public static readonly TizenFeature SENSOR_PHOTOMETER_WAKEUP = new("http://tizen.org/feature/sensor.photometer.wakeup", "bool");
    public static readonly TizenFeature SENSOR_PROXIMITY = new("http://tizen.org/feature/sensor.proximity", "bool");
    public static readonly TizenFeature SENSOR_PROXIMITY_WAKEUP = new("http://tizen.org/feature/sensor.proximity.wakeup", "bool");
    public static readonly TizenFeature SENSOR_ROTATION_VECTOR = new("http://tizen.org/feature/sensor.rotation_vector", "bool");
    public static readonly TizenFeature SENSOR_SIGNIFICANT_MOTION = new("http://tizen.org/feature/sensor.significant_motion", "bool");
    public static readonly TizenFeature SENSOR_SLEEP_MONITOR = new("http://tizen.org/feature/sensor.sleep_monitor", "bool");
    public static readonly TizenFeature SENSOR_STRESS_MONITOR = new("http://tizen.org/feature/sensor.stress_monitor", "bool");
    public static readonly TizenFeature SENSOR_TEMPERATURE = new("http://tizen.org/feature/sensor.temperature", "bool");
    public static readonly TizenFeature SENSOR_TILTMETER = new("http://tizen.org/feature/sensor.tiltmeter", "bool");
    public static readonly TizenFeature SENSOR_TILTMETER_WAKEUP = new("http://tizen.org/feature/sensor.tiltmeter.wakeup", "bool");
    public static readonly TizenFeature SENSOR_ULTRAVIOLET = new("http://tizen.org/feature/sensor.ultraviolet", "bool");
    public static readonly TizenFeature SENSOR_WRIST_UP = new("http://tizen.org/feature/sensor.wrist_up", "bool");

    // Shell
    public static readonly TizenFeature SHELL_APPWIDGET = new("http://tizen.org/feature/shell.appwidget", "bool");

    // SIP
    public static readonly TizenFeature SIP_VOIP = new("http://tizen.org/feature/sip.voip", "bool");

    // Speech
    public static readonly TizenFeature SPEECH_CONTROL = new("http://tizen.org/feature/speech.control", "bool");
    public static readonly TizenFeature SPEECH_CONTROL_MANAGER = new("http://tizen.org/feature/speech.control_manager", "bool");
    public static readonly TizenFeature SPEECH_RECOGNITION = new("http://tizen.org/feature/speech.recognition", "bool");
    public static readonly TizenFeature SPEECH_SYNTHESIS = new("http://tizen.org/feature/speech.synthesis", "bool");

    // Storage
    public static readonly TizenFeature STORAGE_EXTERNAL = new("http://tizen.org/feature/storage.external", "bool");

    // System Setting
    public static readonly TizenFeature SYSTEMSETTING = new("http://tizen.org/feature/systemsetting", "bool");
    public static readonly TizenFeature SYSTEMSETTING_HOME_SCREEN = new("http://tizen.org/feature/systemsetting.home_screen", "bool");
    public static readonly TizenFeature SYSTEMSETTING_INCOMING_CALL = new("http://tizen.org/feature/systemsetting.incoming_call", "bool");
    public static readonly TizenFeature SYSTEMSETTING_LOCK_SCREEN = new("http://tizen.org/feature/systemsetting.lock_screen", "bool");
    public static readonly TizenFeature SYSTEMSETTING_NOTIFICATION_EMAIL = new("http://tizen.org/feature/systemsetting.notification_email", "bool");

    // Thermistor
    public static readonly TizenFeature THERMISTOR_AP = new("http://tizen.org/feature/thermistor.ap", "bool");
    public static readonly TizenFeature THERMISTOR_CP = new("http://tizen.org/feature/thermistor.cp", "bool");
    public static readonly TizenFeature THERMISTOR_BATTERY = new("http://tizen.org/feature/thermistor.battery", "bool");

    // UI Service
    public static readonly TizenFeature UI_SERVICE_STICKER = new("http://tizen.org/feature/ui_service.sticker", "bool");

    // USB
    public static readonly TizenFeature USB_ACCESSORY = new("http://tizen.org/feature/usb.accessory", "bool");
    public static readonly TizenFeature USB_HOST = new("http://tizen.org/feature/usb.host", "bool");

    // Vision
    public static readonly TizenFeature VISION_BARCODE_DETECTION = new("http://tizen.org/feature/vision.barcode_detection", "bool");
    public static readonly TizenFeature VISION_BARCODE_GENERATION = new("http://tizen.org/feature/vision.barcode_generation", "bool");
    public static readonly TizenFeature VISION_FACE_RECOGNITION = new("http://tizen.org/feature/vision.face_recognition", "bool");
    public static readonly TizenFeature VISION_IMAGE_RECOGNITION = new("http://tizen.org/feature/vision.image_recognition", "bool");
    public static readonly TizenFeature VISION_QRCODE_GENERATION = new("http://tizen.org/feature/vision.qrcode_generation", "bool");
    public static readonly TizenFeature VISION_QRCODE_RECOGNITION = new("http://tizen.org/feature/vision.qrcode_recognition", "bool");

    // Web
    public static readonly TizenFeature WEB_IME = new("http://tizen.org/feature/web.ime", "bool");
    public static readonly TizenFeature WEB_SERVICE = new("http://tizen.org/feature/web.service", "bool");

    public static readonly TizenFeature[] All = new TizenFeature[]
    {
        ACCOUNT,
        ACCOUNT_SYNC,
        ACCESSIBILITY_GRAYSCALE,
        ACCESSIBILITY_NEGATIVE,
        APP_HISTORY,
        ATTACH_PANEL,
        BATTERY,
        CAMERA,
        CAMERA_BACK,
        CAMERA_BACK_FLASH,
        CAMERA_FRONT,
        CAMERA_FRONT_FLASH,
        CONSUMER_IR,
        CONTENT_SCANNING_OTHERS,
        CONTENT_FILTER_PINYIN,
        CONTEXTUAL_TRIGGER,
        DATABASE_ENCRYPTION,
        DIAGNOTICS,
        DOWNLOAD,
        FEEDBACK_VIBRATION,
        FIDO_UAF,
        FMRADIO,
        GRAPHICS_ACCELERATION,
        HUMANACTIVITYMONITOR,
        INPUT_KEYBOARD,
        INPUT_KEYBOARD_LAYOUT,
        INPUT_ROTATING_BEZEL,
        INPUT_ROTATING_BEZEL_VIRTUAL,
        IOT_OCF,
        LED,
        LOCATION,
        LOCATION_BATCH,
        LOCATION_GEOFENCE,
        LOCATION_GPS,
        LOCATION_GPS_SATELLITE,
        LOCATION_WPS,
        MACHINE_LEARNING_INFERENCE,
        MACHINE_LEARNING_TRAINING,
        MAPS,
        MEDIA_AUDIO_RECORDING,
        MEDIA_IMAGE_CAPTURE,
        MEDIA_VIDEO_RECORDING,
        MICROPHONE,
        MULTIMEDIA_MEDIA_CODEC,
        MULTIMEDIA_PLAYER_SPHERICAL_VIDEO,
        MULTIMEDIA_STREAM_RECORDER,
        MULTIMEDIA_TRANSCODER,
        MULTI_POINT_TOUCH_PINCH_ZOOM,
        MULTI_POINT_TOUCH_POINT_COUNT,
        NETWORK_BLUETOOTH,
        NETWORK_BLUETOOTH_AUDIO_CALL,
        NETWORK_BLUETOOTH_AUDIO_CONTROLLER,
        NETWORK_BLUETOOTH_AUDIO_MEDIA,
        NETWORK_BLUETOOTH_HEALTH,
        NETWORK_BLUETOOTH_HID,
        NETWORK_BLUETOOTH_HID_DEVICE,
        NETWORK_BLUETOOTH_LE,
        NETWORK_BLUETOOTH_LE_5_0,
        NETWORK_BLUETOOTH_LE_GATT_CLIENT,
        NETWORK_BLUETOOTH_LE_GATT_SERVER,
        NETWORK_BLUETOOTH_LE_IPSP,
        NETWORK_BLUETOOTH_OOB,
        NETWORK_BLUETOOTH_OPP,
        NETWORK_BLUETOOTH_PHONEBOOK_CLIENT,
        NETWORK_ETHERNET,
        NETWORK_INM,
        NETWORK_INTERNET,
        NETWORK_MTP,
        NETWORK_NET_PROXY,
        NETWORK_PUSH,
        NETWORK_VPN,
        NETWORK_NFC,
        NETWORK_NFC_CARD_EMULATION,
        NETWORK_NFC_CARD_EMULATION_HCE,
        NETWORK_NFC_P2P,
        NETWORK_NFC_RESERVED_PUSH,
        NETWORK_NFC_TAG,
        NETWORK_SECURE_ELEMENT,
        NETWORK_SECURE_ELEMENT_ESE,
        NETWORK_SECURE_ELEMENT_UICC,
        NETWORK_SERVICE_DISCOVERY_DNSSD,
        NETWORK_SERVICE_DISCOVERY_SSDP,
        NETWORK_TELEPHONY,
        NETWORK_TELEPHONY_MMS,
        NETWORK_TELEPHONY_SERVICE_CDMA,
        NETWORK_TELEPHONY_SERVICE_EDGE,
        NETWORK_TELEPHONY_SERVICE_GPRS,
        NETWORK_TELEPHONY_SERVICE_GSM,
        NETWORK_TELEPHONY_SERVICE_HSDPA,
        NETWORK_TELEPHONY_SERVICE_HSPA,
        NETWORK_TELEPHONY_SERVICE_HSUPA,
        NETWORK_TELEPHONY_SERVICE_LTE,
        NETWORK_TELEPHONY_SERVICE_UMTS,
        NETWORK_TELEPHONY_SMS,
        NETWORK_TELEPHONY_SMS_CBS,
        NETWORK_TETHERING,
        NETWORK_TETHERING_BLUETOOTH,
        NETWORK_TETHERING_USB,
        NETWORK_TETHERING_WIFI,
        NETWORK_TETHERING_WIFI_DIRECT,
        NETWORK_WIFI,
        NETWORK_WIFI_DIRECT,
        NETWORK_WIFI_DIRECT_DISPLAY,
        NETWORK_WIFI_DIRECT_SERVICE_DISCOVERY,
        NETWORK_WIFI_TDLS,
        OAUTH2,
        OPENGLES,
        OPENGLES_TEXTURE_FORMAT,
        OPENGLES_TEXTURE_FORMAT_3DC,
        OPENGLES_TEXTURE_FORMAT_ATC,
        OPENGLES_TEXTURE_FORMAT_ETC,
        OPENGLES_TEXTURE_FORMAT_PTC,
        OPENGLES_TEXTURE_FORMAT_PVRTC,
        OPENGLES_TEXTURE_FORMAT_UTC,
        OPENGLES_VERSION_1_1,
        OPENGLES_VERSION_2_0,
        OPENGLES_VERSION_3_0,
        OPENGLES_VERSION_3_1,
        OPENGLES_VERSION_3_2,
        PERIPHERAL_IO_GPIO,
        PERIPHERAL_IO_I2C,
        PERIPHERAL_IO_PWM,
        PERIPHERAL_IO_ADC,
        PERIPHERAL_IO_UART,
        PERIPHERAL_IO_SPI,
        PLATFORM_CORE_API_VERSION,
        PLATFORM_CORE_ABI,
        PLATFORM_CORE_CPU_ARCH,
        PLATFORM_CORE_CPU_ARCH_ARMV6,
        PLATFORM_CORE_CPU_ARCH_ARMV7,
        PLATFORM_CORE_CPU_ARCH_ARMV8,
        PLATFORM_CORE_CPU_ARCH_RISCV32,
        PLATFORM_CORE_CPU_ARCH_RISCV64,
        PLATFORM_CORE_CPU_ARCH_X86,
        PLATFORM_CORE_CPU_FREQUENCY,
        PLATFORM_CORE_FPU_ARCH,
        PLATFORM_CORE_FPU_ARCH_SSE2,
        PLATFORM_CORE_FPU_ARCH_SSE3,
        PLATFORM_CORE_FPU_ARCH_SSSE3,
        PLATFORM_CORE_FPU_ARCH_VFPV2,
        PLATFORM_CORE_FPU_ARCH_VFPV3,
        PLATFORM_CORE_FPU_ARCH_VFPV4,
        PLATFORM_NATIVE_API_VERSION,
        PLATFORM_NATIVE_OSP_COMPATIBLE,
        PLATFORM_VERSION,
        PLATFORM_VERSION_NAME,
        PLATFORM_WEB_API_VERSION,
        PROFILE,
        SCREEN,
        SCREEN_ALWAYS_ON_AMOLED,
        SCREEN_ALWAYS_ON_HIGH_COLOR,
        SCREEN_ALWAYS_ON_LOW_BIT_COLOR,
        SCREEN_ALWAYS_ON_VIRTUAL,
        SCREEN_AUTO_ROTATION,
        SCREEN_BPP,
        SCREEN_COORDINATE_SYSTEM_SIZE_LARGE,
        SCREEN_COORDINATE_SYSTEM_SIZE_NORMAL,
        SCREEN_DPI,
        SCREEN_HEIGHT,
        SCREEN_OUTPUT_HDMI,
        SCREEN_OUTPUT_RCA,
        SCREEN_SHAPE_CIRCLE,
        SCREEN_SHAPE_RECTANGLE,
        SCREEN_SIZE_ALL,
        SCREEN_SIZE_LARGE,
        SCREEN_SIZE_NORMAL,
        SCREEN_SIZE_NORMAL_240_400,
        SCREEN_SIZE_NORMAL_320_320,
        SCREEN_SIZE_NORMAL_320_480,
        SCREEN_SIZE_NORMAL_360_360,
        SCREEN_SIZE_NORMAL_360_480,
        SCREEN_SIZE_NORMAL_480_800,
        SCREEN_SIZE_NORMAL_540_960,
        SCREEN_SIZE_NORMAL_600_1024,
        SCREEN_SIZE_NORMAL_720_1280,
        SCREEN_SIZE_NORMAL_1080_1920,
        SCREEN_WIDTH,
        SECURITY_DEVICE_CERTIFICATE,
        SECURITY_PRIVACY_PRIVILEGE,
        SECURITY_TEE,
        SENSOR_ACCELEROMETER,
        SENSOR_ACCELEROMETER_WAKEUP,
        SENSOR_ACTIVITY_RECOGNITION,
        SENSOR_BAROMETER,
        SENSOR_BAROMETER_WAKEUP,
        SENSOR_GEOMAGNETIC_ROTATION_VECTOR,
        SENSOR_GESTURE_RECOGNITION,
        SENSOR_GRAVITY,
        SENSOR_GYROSCOPE,
        SENSOR_GYROSCOPE_ROTATION_VECTOR,
        SENSOR_GYROSCOPE_UNCALIBRATED,
        SENSOR_GYROSCOPE_WAKEUP,
        SENSOR_HEART_RATE_MONITOR,
        SENSOR_HEART_RATE_MONITOR_BATCH,
        SENSOR_HEART_RATE_MONITOR_LED_GREEN,
        SENSOR_HEART_RATE_MONITOR_LED_GREEN_BATCH,
        SENSOR_HEART_RATE_MONITOR_LED_IR,
        SENSOR_HEART_RATE_MONITOR_LED_RED,
        SENSOR_HUMIDITY,
        SENSOR_LINEAR_ACCELERATION,
        SENSOR_MAGNETOMETER,
        SENSOR_MAGNETOMETER_UNCALIBRATED,
        SENSOR_MAGNETOMETER_WAKEUP,
        SENSOR_PEDOMETER,
        SENSOR_PHOTOMETER,
        SENSOR_PHOTOMETER_WAKEUP,
        SENSOR_PROXIMITY,
        SENSOR_PROXIMITY_WAKEUP,
        SENSOR_ROTATION_VECTOR,
        SENSOR_SIGNIFICANT_MOTION,
        SENSOR_SLEEP_MONITOR,
        SENSOR_STRESS_MONITOR,
        SENSOR_TEMPERATURE,
        SENSOR_TILTMETER,
        SENSOR_TILTMETER_WAKEUP,
        SENSOR_ULTRAVIOLET,
        SENSOR_WRIST_UP,
        SHELL_APPWIDGET,
        SIP_VOIP,
        SPEECH_CONTROL,
        SPEECH_CONTROL_MANAGER,
        SPEECH_RECOGNITION,
        SPEECH_SYNTHESIS,
        STORAGE_EXTERNAL,
        SYSTEMSETTING,
        SYSTEMSETTING_HOME_SCREEN,
        SYSTEMSETTING_INCOMING_CALL,
        SYSTEMSETTING_LOCK_SCREEN,
        SYSTEMSETTING_NOTIFICATION_EMAIL,
        THERMISTOR_AP,
        THERMISTOR_CP,
        THERMISTOR_BATTERY,
        UI_SERVICE_STICKER,
        USB_ACCESSORY,
        USB_HOST,
        VISION_BARCODE_DETECTION,
        VISION_BARCODE_GENERATION,
        VISION_FACE_RECOGNITION,
        VISION_IMAGE_RECOGNITION,
        VISION_QRCODE_GENERATION,
        VISION_QRCODE_RECOGNITION,
        WEB_IME,
        WEB_SERVICE,
    };
}
