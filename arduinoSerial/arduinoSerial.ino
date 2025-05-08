bool dispositivoDetectado = false;
bool envioDatosActivo = false;

unsigned long ultimoEnvioDatos = 0;
unsigned long ultimoHBRecibido = 0;
unsigned long ultimoHBEnviado = 0;

bool heartbeatInicialRecibido = false;

void setup() {
  Serial.begin(115200);
  ultimoHBRecibido = millis(); // Para detectar pérdida de conexión
}

void loop() {
  // Recibir comandos desde C#
  if (Serial.available() > 0) {
    String recibido = Serial.readStringUntil('\n');
    recibido.trim();

    if (recibido == "STOP") {
      dispositivoDetectado = true;
    }
    else if (recibido == "RESET") {
      dispositivoDetectado = false;
      envioDatosActivo = false;
       heartbeatInicialRecibido = false;
    }
    else if (recibido == "INICIO") {
      envioDatosActivo = true;
    }
    else if (recibido == "PARAR") {
      envioDatosActivo = false;
    }
    else if (recibido == "HB_PC") {
      ultimoHBRecibido = millis(); // Heartbeat recibido
      heartbeatInicialRecibido = true;
    }
  }

  // Verificar si se perdió el heartbeat desde la PC (desconexión)
   // 💡 NO verifiques el timeout hasta que haya recibido al menos 1 heartbeat
  if (dispositivoDetectado && heartbeatInicialRecibido && (millis() - ultimoHBRecibido > 6000)) {
    dispositivoDetectado = false;
    envioDatosActivo = false;
    heartbeatInicialRecibido = false;
    Serial.println("TIMEOUT: reinicio de protocolo.");
  }

  // Enviar heartbeat desde Arduino cada 2 segundos
  if (millis() - ultimoHBEnviado >= 2000) {
    Serial.println("HB_ARDUINO");
    ultimoHBEnviado = millis();
  }

  // Si no ha sido detectado, enviar identificación
  if (!dispositivoDetectado && millis() - ultimoEnvioDatos >= 2000) {
    Serial.println("DEVICE:ARDUINO");
    ultimoEnvioDatos = millis();
  }

  // Enviar dato si activado
  if (dispositivoDetectado && envioDatosActivo && millis() - ultimoEnvioDatos >= 1000) {
    int temp = random(20, 80);
    Serial.print("$");
    Serial.println(temp);
    ultimoEnvioDatos = millis();
  }
}
