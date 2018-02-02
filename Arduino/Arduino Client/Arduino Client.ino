#include <FastLED.h>
#define NUM_LEDS 288
#define DATA_PIN 2

CRGB leds[NUM_LEDS];

void setup()
{
	Serial.begin(2000000);
	FastLED.addLeds<WS2812B, DATA_PIN, GRB>(leds, NUM_LEDS);
}

void loop()
{
	byte SerialRGBIn[3];

	//wait for serial comms
	while (!Serial.available()) {}

	//read in 3 bytes of color information
	for (int i = 0; i < 3; i++)
		SerialRGBIn[i] = Serial.read();

	//brightness correction, since these LEDs are bright as hell
	FastLED.showColor(CRGB(((int)SerialRGBIn[0]) / 4, ((int)SerialRGBIn[1]) / 4, ((int)SerialRGBIn[2]) / 4));
}
