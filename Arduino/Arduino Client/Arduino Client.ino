#include <FastLED.h>
#define NUM_LEDS 288
#define DATA_PIN 2

CRGB leds[NUM_LEDS];
//TODO: Better Arduino communication
void setup()
{
	Serial.begin(115200);
	FastLED.addLeds<WS2812B, DATA_PIN, GRB>(leds, NUM_LEDS);

	//turn them off when we first start
	FastLED.showColor(CRGB(0, 0, 0));
}

void loop()
{
	byte SerialRGBIn[3];

	//wait for serial comms
	int resetLEDs = 0;
	while (!Serial.available())
	{
		//Set LEDs to black if we haven't gotten sound data
		resetLEDs++;
		if (resetLEDs >= 16000000)
		{
			resetLEDs = 0;
			FastLED.showColor(CRGB(0, 0, 0));
		}
	}

	//read in 3 bytes of color information
	Serial.readBytes(SerialRGBIn, 3);
	for (int i = 0; i < 3; i++)
		SerialRGBIn[i] = round(pow((double)SerialRGBIn[i], 1/1.25));

	//brightness correction, since these LEDs are bright as hell
	FastLED.showColor(CRGB((int)SerialRGBIn[0], (int)SerialRGBIn[1], (int)SerialRGBIn[2]));
}
