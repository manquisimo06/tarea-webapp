# 🛡️ Informe de Revisión OWASP ZAP

Este documento detalla el análisis de seguridad y las correcciones aplicadas al sitio web tras las pruebas de vulnerabilidades.

* **Sitio escaneado:** `http://localhost:5253`
* **Reporte Inicial:** `Informe_ZAP_Antes.html`
* **Reporte Posterior:** `Informe_ZAP_Despues.html`

---

## 📊 Tabla Comparativa: Antes y Después

| # | Hallazgo | Antes (ZAP) | Clasificación | Después (ZAP) |
|---|---|---|---|---|
| **1** | CSP: Directiva Wildcard | Medio (1) | 🟩 **CORREGIDA** | Ausente; CSP restringida y presente. |
| **2** | CSP: Directiva sin fallback (form-action) | Medio (1) | 🟩 **CORREGIDA** | Ausente; `form-action 'self'` está presente. |
| **3** | CSP: script-src unsafe-inline | Medio (1) | 🟩 **CORREGIDA** | Ausente; nonce coincide y no hay unsafe-inline. |
| **4** | CSP: style-src unsafe-inline | Medio (1) | 🟩 **CORREGIDA** | Ausente; sin estilos inline en HTML. |
| **5** | Falta X-Content-Type-Options | Bajo (1) | 🟩 **CORREGIDA** | Ausente; `nosniff` incluido en respuestas. |
| **6** | Aplicación Web Moderna | Informativo | 🟦 **INFORMATIVA** | Persiste como observación, no vulnerabilidad. |
| **7** | Comentarios sospechosos | Informativo | 🟦 **INFORMATIVA** | Persiste; comentario genérico no sensible. |
| **8** | Respuesta de Gestión de Sesión | Informativo | 🟦 **INFORMATIVA** | Persiste; cookie antifalsificación esperada. |

> **💡 Resultado comparativo:** El resumen por riesgo pasó de **4 medias y 1 baja** a **0 altas, 0 medias y 0 bajas**. Las cinco alertas accionables han sido completamente solventadas y solo permanecen las 3 observaciones informativas.

> **⚠️ Nota de consistencia para la entrega:** En `Informe_ZAP_Despues.html`, el resumen y la sección *Alerts* indican 3 alertas informativas. La tabla *Alert Counts by Alert Type* muestra un desglose de 2, 1 y 2 (suma 5), aunque su pie vuelve a indicar un total de 3. Se conserva el total del resumen por riesgo y se documenta esta discrepancia del reporte original para conocimiento de la revisión.

---

## 🛠️ Vulnerabilidades Corregidas

### 1. CSP: Directiva Wildcard *(Riesgo Medio)*
Se reemplazó la política incompleta por una cabecera `Content-Security-Policy` con directivas explícitas y fuentes limitadas. Se conservaron `frame-ancestors 'self'` y `X-Frame-Options SAMEORIGIN`. La política ahora restringe de manera segura: `default-src`, `base-uri`, `object-src`, `form-action`, `script-src`, `style-src`, `img-src`, `connect-src`, `frame-src`, `font-src`, `media-src`, `manifest-src` y `worker-src`.

### 2. CSP: Failure to Define Directive with No Fallback *(Riesgo Medio)*
Se agregó la directiva `form-action 'self'`, la cual no hereda su restricción desde `default-src`. Adicionalmente, se declararon todas las demás directivas que ZAP marcó previamente como ausentes o demasiado amplias.

### 3. CSP: script-src unsafe-inline *(Riesgo Medio)*
Se implementó el uso de un **nonce** aleatorio por respuesta para el *ImportMap* de Blazor. La directiva `script-src` ahora permite únicamente scripts del mismo origen y el uso del nonce válido; `script-src-attr` se configuró en `none`. Se removió exitosamente el manejador JavaScript inline del menú y se reemplazó por un evento nativo de Blazor.

### 4. CSP: style-src unsafe-inline *(Riesgo Medio)*
La directiva `style-src` quedó limitada al mismo origen (`'self'`) y `style-src-attr` se fijó en `none`. Los estilos inline de la página *Clientes* se movieron limpiamente hacia su aislamiento correspondiente en `Components/Pages/Cliente.razor.css`. Se comprobó que los estilos visuales (como el fondo del modal) siguen operando correctamente.

### 5. Falta encabezado X-Content-Type-Options *(Riesgo Bajo)*
Se inyecta forzosamente el encabezado `X-Content-Type-Options: nosniff` a todas las respuestas del servidor, incluyendo los archivos estáticos. Se validó su correcta recepción inspeccionando elementos como el `favicon`, el cual responde con estado `200`, `Content-Type: image/png` y la cabecera de seguridad activa.

---

## 🔍 Hallazgos Informativos (No requerían corrección)

* **6. Aplicación Web Moderna:** Es una alerta de contexto tecnológico emitida por ZAP, no representa ningún riesgo ni vulnerabilidad explotable.
* **7. Divulgación de información - Comentarios sospechosos:** ZAP detectó la palabra *"user"* dentro de un fragmento de código del módulo de reconexión. Dado que es un comentario genérico del framework y no expone secretos, credenciales o datos sensibles, se mantuvo en el proyecto.
* **8. Respuesta de Gestión de Sesión Identificada:** Corresponde de forma legítima a la cookie antifalsificación de ASP.NET Core utilizada para mitigar ataques CSRF. El reporte confirma que cuenta con los flags de seguridad idóneos: `HttpOnly` y `SameSite=Strict`.

---

## ⚙️ Cambios Previos (Fuera del alcance de ZAP)
Se mantuvieron intactas las siguientes configuraciones previas del sistema que no derivan de los hallazgos de seguridad:
* Corrección de la URL base para la API de Clientes.
* Mapeo de propiedades JSON utilizando formato `snake_case`.
* Enrutamiento correcto del enlace del menú hacia `/clientes`.

---

## ✅ Verificación de Pruebas y Pendientes

- [x] Ejecución de compilación limpia: `dotnet build --no-restore --output "%TEMP%\WebEmpresa-zap-check" -p:UseAppHost=false` sin errores.
- [x] Verificación de la coincidencia dinámica del `nonce` en la cabecera `script-src` y el atributo del HTML.
- [x] Comprobación del correcto funcionamiento de la página de Clientes y la carga del formulario "Nuevo" bajo las restricciones de la CSP.
- [x] Escaneo secundario de ZAP completado de manera exitosa confirmando **0 alertas explotables** restantes.
- [ ] **Siguiente paso recomendado:** Dado que el análisis inicial se ejecutó sobre un entorno local (`HTTP` en `localhost`), se sugiere planificar una repetición del escaneo automatizado contra el entorno definitivo de producción bajo protocolo seguro (`HTTPS`).
