# Spec de Seguridad — Aplicaciones Móviles (Android / iOS)
**Frente:** Móvil · **Base:** OWASP Mobile Top 10 2024 + MASVS · **Núcleo:** secure-core.md (R1–R7)
**Formato:** EARS + criterios de aceptación · **Trazabilidad:** OWASP + Núcleo + Prueba (MASTG/MASVS)
**Consumido por:** agentes de IA de generación y revisión de código

> Premisa MASVS: el dispositivo es HOSTIL. Todo secreto o decisión de seguridad que viva SOLO en
> el cliente se considera comprometido. La seguridad real se valida en el servidor.

---

## REQ-MOB-01 — Improper Credential Usage
**OWASP: M1:2024 · Núcleo: R1 · Prueba: MASTG "Testing for Hardcoded Secrets" (MASVS-STORAGE-1) · Severidad: CRÍTICA**

THE SYSTEM SHALL no contener credenciales, claves ni secretos embebidos en el binario, recursos
o código fuente.

Criterios de aceptación:
- [ ] Cero claves de cifrado, API keys, contraseñas o tokens hardcodeados en el APK/IPA.
- [ ] Los secretos se obtienen en runtime desde backend autenticado o el almacén seguro del SO.
- [ ] Análisis estático del binario (strings/jadx) no revela secretos.

---

## REQ-MOB-02 — Supply Chain Security
**OWASP: M2:2024 · Núcleo: A06 · Prueba: MASTG "Testing the App Signature" (MASVS-RESILIENCE-1) · Severidad: ALTA**

THE SYSTEM SHALL controlar la procedencia e integridad de sus dependencias y del binario.

Criterios de aceptación:
- [ ] SDKs/librerías con versión pinneada y origen verificado.
- [ ] El binario de release está firmado.
- [ ] SCA ejecutado sobre las dependencias; sin vulnerabilidades HIGH/CRITICAL sin mitigar.

---

## REQ-MOB-03 — Secure Authentication/Authorization
**OWASP: M3:2024 · Núcleo: R3, R6 · Prueba: MASTG "Testing Local Auth" (MASVS-AUTH-1) · Severidad: CRÍTICA**

WHEN la app toma una decisión de autenticación o autorización,
THE SYSTEM SHALL delegar la decisión final al servidor.

Criterios de aceptación:
- [ ] La autorización se valida server-side; el cliente no concede permisos.
- [ ] El estado "autenticado" no se persiste como flag local manipulable.
- [ ] Tokens validados (firma+exp+iss+aud); sin PII en el token.

---

## REQ-MOB-04 — Input/Output Validation
**OWASP: M4:2024 · Núcleo: R5 · Prueba: MASTG "Testing for Injection Flaws" (MASVS-PLATFORM-2) · Severidad: ALTA**

WHEN la app recibe datos de una fuente externa (servidor, deep link, intent, otra app),
THE SYSTEM SHALL validar y sanitizar esos datos antes de usarlos.

Criterios de aceptación:
- [ ] Validación de datos del servidor, deep links e intents contra esquema esperado.
- [ ] Componentes exportados (Android) protegidos; no aceptan input arbitrario sin validar.

---

## REQ-MOB-05 — Secure Communication
**OWASP: M5:2024 · Núcleo: R2 · Prueba: MASTG "Testing Network Comm / Pinning" (MASVS-NETWORK-1) · Severidad: CRÍTICA**

WHILE la app transmite datos,
THE SYSTEM SHALL usar TLS 1.2+ y NUNCA transmitir en texto claro.

Criterios de aceptación:
- [ ] Todo el tráfico sobre TLS 1.2+; cleartext deshabilitado (cleartextTrafficPermitted=false).
- [ ] Certificate pinning implementado.
- [ ] No se implementa cifrado propietario como sustituto de TLS.

---

## REQ-MOB-06 — Privacy Controls
**OWASP: M6:2024 · Núcleo: R6 · Prueba: MASTG "Testing for Sensitive Data in Logs" (MASVS-PRIVACY-1) · Severidad: ALTA**

THE SYSTEM SHALL minimizar la recolección, almacenamiento y exposición de PII.

Criterios de aceptación:
- [ ] Solo se recolecta la PII estrictamente necesaria.
- [ ] La PII no se escribe en logs ni se incluye en el token.
- [ ] Consentimiento y propósito documentados para cada dato sensible recolectado.

---

## REQ-MOB-07 — Binary Protections
**OWASP: M7:2024 · Núcleo: A05 · Prueba: MASTG "Testing Anti-Tampering / Anti-Debugging" (MASVS-RESILIENCE-2) · Severidad: MEDIA**

WHERE la app maneja lógica o datos sensibles,
THE SYSTEM SHALL incorporar protecciones del binario contra ingeniería inversa y manipulación.

Criterios de aceptación:
- [ ] Ofuscación de código en release.
- [ ] Detección de root/jailbreak y anti-tampering/anti-debugging.
- [ ] Herramientas de desarrollo/debug DESHABILITADAS en builds de producción.

---

## REQ-MOB-08 — Security Misconfiguration
**OWASP: M8:2024 · Núcleo: A05 · Prueba: MASTG "Testing for Debuggable Flag / Backup" (MASVS-PLATFORM-1) · Severidad: MEDIA**

THE SYSTEM SHALL desplegarse con una configuración endurecida y permisos mínimos.

Criterios de aceptación:
- [ ] Sin flags de debug, logging verboso ni endpoints de test en release.
- [ ] Permisos de la app reducidos al mínimo necesario.
- [ ] `android:allowBackup=false` y configuración de seguridad de red correcta.

---

## REQ-MOB-09 — Insecure Data Storage
**OWASP: M9:2024 · Núcleo: R1 · Prueba: MASTG "Testing Local Storage for Sensitive Data" (MASVS-STORAGE-1) · Severidad: ALTA**

WHEN la app almacena datos sensibles en el dispositivo,
THE SYSTEM SHALL usar el almacén seguro del SO y nunca persistir secretos en claro.

Criterios de aceptación:
- [ ] Secretos en Android Keystore / iOS Keychain, no en SharedPreferences/plist/SQLite plano.
- [ ] PII no se cachea innecesariamente; se limpia al cerrar sesión.
- [ ] Sin datos sensibles en logs, backups ni capturas de pantalla.

---

## REQ-MOB-10 — Sufficient Cryptography
**OWASP: M10:2024 · Núcleo: R2 · Prueba: MASTG "Testing Symmetric Cryptography" (MASVS-CRYPTO-1) · Severidad: CRÍTICA**

WHEN la app cifra datos,
THE SYSTEM SHALL usar criptografía fuerte del SO con IV/nonce aleatorio por operación.

Criterios de aceptación:
- [ ] AES-256-GCM o ChaCha20-Poly1305 vía Keystore/Keychain; IV aleatorio por operación.
- [ ] Las claves no se derivan de valores predecibles; IV ≠ clave; sin ECB.
- [ ] El cifrado es no-determinístico (mismo plaintext → distinto ciphertext).

---

## Cobertura OWASP Mobile Top 10 2024
M1→REQ-MOB-01 · M2→REQ-MOB-02 · M3→REQ-MOB-03 · M4→REQ-MOB-04 · M5→REQ-MOB-05 ·
M6→REQ-MOB-06 · M7→REQ-MOB-07 · M8→REQ-MOB-08 · M9→REQ-MOB-09 · M10→REQ-MOB-10.
