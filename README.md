# 🔌 Conexión Serial Automática Arduino / ESP32 + Visual C#

Este repositorio implementa una solución de conexión automática entre un dispositivo Arduino o ESP32 y una aplicación en **Visual C# Windows Forms**. Incluye detección plug & play, comunicación bidireccional por puerto serial, gráficas en tiempo real y manejo de desconexión automática.

---

## 📁 Estructura del repositorio
## 📂 arduinoSerial → Código del Arduino o ESP32
## 📂 conexionSerial → Proyecto en Visual Studio (C# Windows Forms)


---

## 🧠 ¿Qué hace este proyecto?

✅ Detecta automáticamente el puerto COM al conectar el Arduino/ESP32  
✅ Envía comandos como `INICIO`, `PARAR`, `RESET` y `STOP`  
✅ Muestra notificaciones tipo Windows (📢 NotifyIcon)  
✅ Grafica en tiempo real los datos de temperatura enviados desde el Arduino 📊  
✅ Implementa un sistema de `heartbeat` para detectar desconexión 🔄  
✅ Usa eventos del sistema operativo para detectar dispositivos (Plug & Play)

---

## 📷 Vista previa

> *(Agrega aquí una imagen de la interfaz con `chart1`, botones y gráfica en vivo)*

---

## ⚙️ Requisitos

- Visual Studio 2022 o superior  
- .NET Framework (Windows Forms)  
- Referencia a `System.Management` (para Plug & Play)  
- Arduino UNO, Nano, ESP32 u otro con salida serial  
- Arduino IDE para cargar el sketch

---

## ▶️ Cómo usar

### 🖥️ En la PC (C#)

1. Abre la carpeta `conexionSerial` en Visual Studio  
2. Asegúrate de que el proyecto tenga agregada la referencia a `System.Management`  
3. Ejecuta la aplicación  
4. Al conectar el Arduino, este será detectado automáticamente  
5. Presiona `Inicio` para comenzar a graficar

### 🔁 En el Arduino

1. Abre la carpeta `arduinoSerial`  
2. Carga el código `.ino` en tu dispositivo desde el IDE de Arduino  
3. Asegúrate de que esté enviando datos tipo `$76` cada segundo tras recibir el comando `INICIO`

---

## 📡 Comunicación serial

- `DEVICE:ESP32` / `DEVICE:ARDUINO`: identificación inicial  
- `STOP`: detiene identificación  
- `INICIO`: comienza a enviar datos `$`  
- `PARAR`: detiene envío de datos  
- `RESET`: reinicia el protocolo  
- `HB_PC` / `HB_ARDUINO`: mensajes de heartbeat

---

## 🎓 Créditos

Proyecto creado por [nito1990](https://github.com/nito1990)  
💡 Inspirado en la necesidad de simplificar la interacción Serial para proyectos IoT y makers

---

## 📜 Licencia

MIT License. Libre uso educativo y profesional. ¡Mención apreciada! 😄
