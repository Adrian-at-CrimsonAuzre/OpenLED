#include <FastLED.h>
#include "Types.h"
#define NUM_LEDS 288
#define DATA_PIN 2

CRGB leds[NUM_LEDS];
ArduinoModes Mode = ArduinoModes::StaticColor;
long resetLEDs = 0;

//TODO: Better Arduino communication
void setup()
{
	//Max BAUD we can get out of a Nano
	Serial.begin(2000000);
	FastLED.addLeds<WS2812B, DATA_PIN, GRB>(leds, NUM_LEDS);

	//turn them off when we first start
	FastLED.showColor(CRGB(0, 0, 0));
}

void loop()
{
	byte SerialRGBIn[3] = { 0,0,0 };

	if (Serial.available())
	{
		//All serial communication will start with a byte indicating the mode to be used
		Mode = (ArduinoModes)Serial.read();

		switch (Mode)
		{
			case(ArduinoModes::Off):
			{
				SerialRGBIn[0] = 0;
				SerialRGBIn[1] = 0;
				SerialRGBIn[2] = 0;
			}

			//Since Color reactive is just a stream of Static Colors, they do the same thing
			case(ArduinoModes::StaticColor):
			case(ArduinoModes::ColorReactive):
			{
				Serial.readBytes(SerialRGBIn, 3);
				break;
			}
			default:
				break;
		}

		//Sometimes the serial buffer gets missaligned (Windows is strange when CPU usage is high) and will incorrectly read data
		//Example:
		//	What is Output:		....28],[7,26,200,65],[7,26,6,200],[7,....
		//	What is Read:			..28,7],[26,200,65,7],[26,6,200,7],[......
		serialFlush();

		//Correct brightness, since the LEDs are bright as hell
		for (int i = 0; i < 3; i++)
			SerialRGBIn[i] = round(pow((double)SerialRGBIn[i], 1 / 1.25));
		FastLED.showColor(CRGB((int)SerialRGBIn[0], (int)SerialRGBIn[1], (int)SerialRGBIn[2]));
	}


	//Some modes (ColorReactive and Visualizer) require a relatively constant stream of data. If we don't get that, turn off the LEDs
	resetLEDs++;
	if (Mode == ArduinoModes::ColorReactive && resetLEDs >= 100000)
	{
		resetLEDs = 0;
		FastLED.showColor(CRGB(0, 0, 0));
	}
}

//Flush serial in
void serialFlush() {
	while (Serial.available() > 0) {
		char t = Serial.read();
	}
}
