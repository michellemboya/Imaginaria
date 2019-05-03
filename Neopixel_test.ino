#include <Uduino.h>
Uduino uduino("PixelBoard");
// NeoPixel Ring simple sketch (c) 2013 Shae Erisson
// Released under the GPLv3 license to match the rest of the
// Adafruit NeoPixel library

#include <Adafruit_NeoPixel.h>
#ifdef __AVR__
 #include <avr/power.h> // Required for 16 MHz Adafruit Trinket
#endif

// Which pin on the Arduino is connected to the NeoPixels?
#define PIN        6 // On Trinket or Gemma, suggest changing this to 1

// How many NeoPixels are attached to the Arduino?
#define NUMPIXELS 24 // Popular NeoPixel ring size

// When setting up the NeoPixel library, we tell it how many pixels,
// and which pin to use to send signals. Note that for older NeoPixel
// strips you might need to change the third parameter -- see the
// strandtest example for more information on possible values.
Adafruit_NeoPixel pixels(NUMPIXELS, PIN, NEO_GRB + NEO_KHZ800);

#define DELAYVAL 100 // Time (in milliseconds) to pause between pixels
int brightness = 0;    // how bright the LED is
int fadeAmount = 5;

void setup() {

  Serial.begin(9600);
  // These lines are specifically to support the Adafruit Trinket 5V 16 MHz.
  // Any other board, you can remove this part (but no harm leaving it):
#if defined(__AVR_ATtiny85__) && (F_CPU == 16000000)
  clock_prescale_set(clock_div_1);
#endif
  // END of Trinket-specific code.
 pixels.setBrightness(250);

  pixels.begin(); // INITIALIZE NeoPixel strip object (REQUIRED)
  uduino.addCommand("redPixels", redPixels);
  uduino.addCommand("bluePixels", bluePixels);

  
}
void loop() {
  uduino.update();
  delay(10);
}

 void redPixels()
 {
  for (int i = 0; i <NUMPIXELS; i++)
  {
  pixels.setPixelColor(i, 255, 69, 0);
  pixels.show();
  }

  pixels.setBrightness(brightness);

  // change the brightness for next time through the loop:
  brightness = brightness + fadeAmount;

  // reverse the direction of the fading at the ends of the fade:
  if (brightness <= 0 || brightness >= 255) {
    fadeAmount = -fadeAmount;
  }
  // wait for 30 milliseconds to see the dimming effect
  delay(30);
  
  }


 void bluePixels() {

  for (int i = 0; i <NUMPIXELS; i++)
  {
  pixels.setPixelColor(i, 51, 85, 255);
  pixels.show();
  }

  pixels.setBrightness(brightness);

  // change the brightness for next time through the loop:
  brightness = brightness + fadeAmount;

  // reverse the direction of the fading at the ends of the fade:
  if (brightness <= 0 || brightness >= 255) {
    fadeAmount = -fadeAmount;
  }
  // wait for 30 milliseconds to see the dimming effect
  delay(30);
  
  }
 
 
