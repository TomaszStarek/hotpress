
int odczytanaWartosc = 0;//Odczytana wartość z ADC
float napiecie = 0;//Wartość przeliczona na napięcie w V
float temperatura_przeskalowana = 0; //Wartość przeliczona z napiecia na temp

int odczytanaWartosc2 = 0;//Odczytana wartość z ADC
float napiecie2 = 0;//Wartość przeliczona na napięcie w V
float temperatura_przeskalowana2 = 0; //Wartość przeliczona z napiecia na temp

char malpa;

void setup() {
  pinMode(2, INPUT_PULLUP);
  pinMode(4, INPUT_PULLUP);
  pinMode(8, OUTPUT);
  digitalWrite(8, LOW); //Pomarańczowa
  Serial.begin(9600);//Uruchomienie komunikacji przez USART
}
 
void loop() {

  
while(Serial.available() > 0) 
{
  malpa = Serial.read();
  
    if(malpa=='@')
    {
      digitalWrite(8, HIGH); //Pomarańczowa
      delay(1000);
    }
    else
      digitalWrite(8, LOW); 
}
     digitalWrite(8, LOW);

     
  odczytanaWartosc = analogRead(A2);//Odczytujemy wartość napięcia
  napiecie = odczytanaWartosc * (5.0/1024.0); //Przeliczenie wartości na napięcie
  temperatura_przeskalowana = napiecie * (450.0/4.425) - 160;


  odczytanaWartosc2 = analogRead(A4);//Odczytujemy wartość napięcia
  napiecie2 = odczytanaWartosc2 * (5.0/1024.0); //Przeliczenie wartości na napięcie
  temperatura_przeskalowana2 = napiecie2 * (450.0/4.425) - 160;

if(digitalRead(2) == LOW && digitalRead(4) == LOW)
{
  delay(100);
  if(digitalRead(2) == LOW && digitalRead(4) == LOW)
  {
    Serial.println("OK");//Wysyłamy zmierzone napięcie
    delay(500);
  }
  else
  {
    Serial.println("BAD");//Wysyłamy zmierzone napięcie
    delay(500);
  }
}
else
{
  Serial.println("BAD");//Wysyłamy zmierzone napięcie
  delay(500);
}


// Serial.println(odczytanaWartosc);//Wysyłamy zmierzone napięcie
// Serial.println(napiecie);//Wysyłamy zmierzone napięcie
 Serial.print("tempA2: ");
 Serial.println(temperatura_przeskalowana);//Wysyłamy zmierzone napięcie
 delay(100);
 Serial.print("tempA4: ");
 Serial.println(temperatura_przeskalowana2);//Wysyłamy zmierzone napięcie
 delay(100);//Czekamy, aby wygodniej odczytywać wyniki  
}     
