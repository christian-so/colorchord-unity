#include "FastLED.h"

#define PIN 4
#define SIZE 75
#define BRIGHTNESS 80

CRGB leds[SIZE];
int i, j;

void setup() {
  FastLED.addLeds<WS2812B, PIN, GRB>(leds, SIZE);
  FastLED.setBrightness(BRIGHTNESS);
  for(int i=0;i<SIZE;i++){
      leds[i].r = 0;
      leds[i].g = 0;
      leds[i].b = 127;
  }
  FastLED.show();
  Serial.begin(115200);
  while(!Serial){}
}




void loop() {
   for( i = 0; i < SIZE; i++){
    for( j = 0; j < 3; j++){
      while(Serial.available() < 1){}
      leds[i][j] = Serial.read();
    }
  }
  FastLED.show();
}
    
   
