#include <FastLED.h>
#include "Types.h"
#define NUM_LEDS 288
#define DATA_PIN 2

CRGB leds[NUM_LEDS];
ArduinoModes Mode = ArduinoModes::StaticColor;
int EffectSpeedMS = 0;
unsigned long EffectCounter = 0;
long resetLEDs = 0;


//Mode, Color1(3), Color2(3), Speed
byte SerialBuffer[8];

//Colors for dual color effects
CRGB ColorOne;
CRGB ColorTwo;
CRGB ColorOut;

//TODO: Better Arduino communication
void setup()
{
	//Max BAUD we can get out of a Nano, Expect 8 bits with Even parity and 2 stop bits
	Serial.begin(2000000);
	Serial.setTimeout(10);
	FastLED.addLeds<WS2812B, DATA_PIN, GRB>(leds, NUM_LEDS);

	//turn them off when we first start
	FastLED.showColor(CRGB(0, 0, 0));
}

void loop()
{
	if (Serial.available())
	{
		//All serial communication will start with a byte indicating the mode to be used
		Serial.readBytes(SerialBuffer, 8);
		Mode = (ArduinoModes)SerialBuffer[0];

		switch (Mode)
		{
			case(ArduinoModes::Off):
			{
				ColorOne = CRGB(0, 0, 0);
				ColorOut = ColorOne;
			}
			//Since Color reactive is just a stream of Static Colors, they do the same thing
			case(ArduinoModes::StaticColor):
			case(ArduinoModes::ColorReactive):
			{
				ColorOne = CRGB(SerialBuffer[1], SerialBuffer[2], SerialBuffer[3]);
				ColorTwo = CRGB(0, 0, 0);

				ColorOut = ColorOne;
				resetLEDs = 0;
				break;
			}
			//All of these require two colors, and a speed
			case(ArduinoModes::Breathing):
			case(ArduinoModes::Cycle):
			case(ArduinoModes::HeartBeat):
			case(ArduinoModes::Rainbow):
			case(ArduinoModes::Strobe):
			{
				ColorOne = CRGB(SerialBuffer[1], SerialBuffer[2], SerialBuffer[3]);
				ColorTwo = CRGB(SerialBuffer[4], SerialBuffer[5], SerialBuffer[6]);
				EffectSpeedMS = SerialBuffer[7];
			}
			default:
				break;
		}

		//Write our color (when this is moved out of this statement, everything is fucked
		FastLED.showColor(ColorOut);
	}

	//Update our dual color effects (if we have any)
	switch (Mode)
	{
		case(ArduinoModes::Breathing):
		{
			EffectCounter++;
			double Correction = (sin(EffectCounter*PI*.01*((double)EffectSpeedMS / 255)) + .5) / 2;
			if (Correction < 0)
				Correction = 0;
			ColorOut = CRGB(round(ColorOne.r * Correction), round(ColorOne.g * Correction), round(ColorOne.b * Correction));
			//HACK
			FastLED.showColor(ColorOut);
			break;
		}
		case(ArduinoModes::Cycle):
		{
			break;
		}
		case(ArduinoModes::HeartBeat):
		{
			break;
		}
		case(ArduinoModes::Rainbow):
		{
			break;
		}
		case(ArduinoModes::Strobe):
		{
			break;
		}
		default:
			break;
	}
	//Some modes (ColorReactive and Visualizer) require a relatively constant stream of data. If we don't get that, turn off the LEDs
	if (Mode == ArduinoModes::ColorReactive)
	{
		resetLEDs++;
		if (resetLEDs >= 100000)
		{
			resetLEDs = 0;
			ColorOut = CRGB(0, 0, 0);
			FastLED.showColor(ColorOut);
		}
	}
}