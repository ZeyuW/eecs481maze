/*
  Analog Input
 Read analog input from five analog pins
 As long as there is sensor input from a pin whose value is larger than a limit,
 that corresponding pin index will be print out
 
 
 Created by Xiaoyi Yu
 modified 31 Oct 2014
 By Xiaoyi Yu

 */
int pinSize = 5;
int threshold = 5;
int sensorPin[] = {A0, A1, A2, A4, A5};    // select the input pin for the potentiometer
int sensorValue[] = {0, 0, 0, 0, 0};  // variable to store the value coming from the sensor
int pinIndex[] = {0, 1, 2, 3, 5};    // variable to indicate the pin index

void setup() {
  // initialize serial communications at 9600 bps:
  Serial.begin(9600);
}

void loop() {
  // read the value from the sensor:
  for (int i = 0; i < pinSize; i++) {
    sensorValue[i] = analogRead(sensorPin[i]);  
  }
  // stop the program for 10 milliseconds:
  delay(10);
  
  // check every pin to see which pin has analog input
  for (int i = 0; i < pinSize; i++) {
    // if touch input is obvious enough, print out the corresponding pin index
    if (sensorValue[i]>=threshold) {
      Serial.println(pinIndex[i]); 
    }
  }
}
