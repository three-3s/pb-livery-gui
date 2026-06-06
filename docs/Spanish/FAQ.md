# Preguntas y respuestas

## ¿Qué idiomas son compatibles?

El mod incluye traducciones básicas para todos los idiomas que el juego admite actualmente.

<table>
  <tr>
    <td>English</td>
    <td><a href="https://github.com/three-3s/pb-livery-gui/blob/main/README.md">README</a></td>
    <td><a href="https://github.com/three-3s/pb-livery-gui/blob/main/FAQ.md">FAQ</a></td>
  </tr>
  <tr>
    <td>简体中文</td>
    <td><a href="https://github.com/three-3s/pb-livery-gui/blob/main/docs/ChineseSimplified/README.md">说明文档</a></td>
    <td><a href="https://github.com/three-3s/pb-livery-gui/blob/main/docs/ChineseSimplified/FAQ.md">常见问题</a></td>
  </tr>
  <tr>
    <td>繁體中文</td>
    <td><a href="https://github.com/three-3s/pb-livery-gui/blob/main/docs/ChineseTraditional/README.md">說明文件</a></td>
    <td><a href="https://github.com/three-3s/pb-livery-gui/blob/main/docs/ChineseTraditional/FAQ.md">常見問題</a></td>
  </tr>
  <tr>
    <td>Français</td>
    <td><a href="https://github.com/three-3s/pb-livery-gui/blob/main/docs/French/README.md">Lisez-moi</a></td>
    <td><a href="https://github.com/three-3s/pb-livery-gui/blob/main/docs/French/FAQ.md">FAQ</a></td>
  </tr>
  <tr>
    <td>Deutsch</td>
    <td><a href="https://github.com/three-3s/pb-livery-gui/blob/main/docs/German/README.md">Liesmich</a></td>
    <td><a href="https://github.com/three-3s/pb-livery-gui/blob/main/docs/German/FAQ.md">Häufige Fragen</a></td>
  </tr>
  <tr>
    <td>Italiano</td>
    <td><a href="https://github.com/three-3s/pb-livery-gui/blob/main/docs/Italian/README.md">Leggimi</a></td>
    <td><a href="https://github.com/three-3s/pb-livery-gui/blob/main/docs/Italian/FAQ.md">Domande frequenti</a></td>
  </tr>
  <tr>
    <td>日本語</td>
    <td><a href="https://github.com/three-3s/pb-livery-gui/blob/main/docs/Japanese/README.md">はじめに</a></td>
    <td><a href="https://github.com/three-3s/pb-livery-gui/blob/main/docs/Japanese/FAQ.md">よくある質問</a></td>
  </tr>
  <tr>
    <td>한국어</td>
    <td><a href="https://github.com/three-3s/pb-livery-gui/blob/main/docs/Korean/README.md">읽어보기</a></td>
    <td><a href="https://github.com/three-3s/pb-livery-gui/blob/main/docs/Korean/FAQ.md">자주 묻는 질문</a></td>
  </tr>
  <tr>
    <td>Português Brasileiro</td>
    <td><a href="https://github.com/three-3s/pb-livery-gui/blob/main/docs/PortugueseBrazilian/README.md">Leia-me</a></td>
    <td><a href="https://github.com/three-3s/pb-livery-gui/blob/main/docs/PortugueseBrazilian/FAQ.md">Perguntas frequentes</a></td>
  </tr>
  <tr>
    <td>Русский</td>
    <td><a href="https://github.com/three-3s/pb-livery-gui/blob/main/docs/Russian/README.md">Прочитайте</a></td>
    <td><a href="https://github.com/three-3s/pb-livery-gui/blob/main/docs/Russian/FAQ.md">Часто задаваемые вопросы</a></td>
  </tr>
  <tr>
    <td>Español</td>
    <td><a href="https://github.com/three-3s/pb-livery-gui/blob/main/docs/Spanish/README.md">Léeme</a></td>
    <td><a href="https://github.com/three-3s/pb-livery-gui/blob/main/docs/Spanish/FAQ.md">Preguntas frecuentes</a></td>
  </tr>
</table>
## ¿Tengo que empezar una partida nueva?

No. Este mod debería ser totalmente compatible con partidas guardadas existentes.

## ¿Puedo seguir usando otros mods que añaden pinturas?

Sí. No podrás sobrescribirlas, pero puedes copiarlas y editar las copias.

## ¿Dónde se guardan los archivos de pintura?

AppData/Local/PhantomBrigade/ModSavedData/LiveryGUI/*.yaml

Por ejemplo:
C:/Users/yourUserID/AppData/Local/PhantomBrigade/ModSavedData/LiveryGUI

Puedes empaquetar estos archivos en un mod de pinturas.

## ¿Es posible usar valores mayores o menores que los permitidos por los deslizadores?

Sí, pero el mod no lo admite directamente por ahora. Al menos para los colores, los valores negativos rompen el color y lo vuelven negro. Los colores demasiado brillantes también acaban rompiendo el color y empiezan a mostrar zonas negras cada vez más grandes. Los parámetros W de material pueden causar fallos importantes de renderizado, especialmente con ciertos valores XYZ de efecto.

## ¿Puedo cambiar una pintura integrada o una pintura proporcionada por otros mods?

No directamente, pero puedes guardar una copia de la pintura y editar esa copia.

## ¿Qué pasa si desinstalo este mod?

Debería ir bien. Las pinturas que ya no existan se verán como pinturas predeterminadas. Pero puede que tengas que reasignar las piezas afectadas para usar otras pinturas, o hacer clic derecho en la página de pinturas para limpiar las pinturas y dejarlas en sin pintura. Si tenías una antigua pintura de torso superior que ya no existe, cambiar la pintura de todo el Meca o de todo el torso puede dejar el torso superior atascado en la pintura predeterminada hasta que se limpie con clic derecho.

Aparte de eso, el juego parece gestionar bastante bien la desactivación o desinstalación del mod.

## ¿Veo grandes círculos blancos?

El intervalo normal de valores está entre 0 y 1. Cualquier otra cosa es experimental.

La causa principal del fallo de renderizado de los grandes círculos blancos es tener un valor W de material principal, secundario o terciario demasiado grande, positivo o negativo. A veces puedes evitarlo ajustando el deslizador XYZ del efecto correspondiente (para efecto: X=principal, Y=secundario, Z=terciario), o reduciendo el color RGB de la pieza problemática (haciendo que R+G+B sea menor). Reducir RGB puede oscurecerla, pero parte del brillo puede recuperarse usando un componente A negativo para ese color.

Nota: los grandes círculos blancos pueden ser visibles solo desde ciertos ángulos, así que usa los botones de mover cámara (WASD por defecto en teclados QWERTY) para examinar el Meca desde varios ángulos. Intenta pasarte un poco del punto donde desaparecen los círculos blancos, para ayudar a que no reaparezcan justo desde el ángulo adecuado.

## ¿Los deslizadores Supporter DLC no hacen nada?

Los efectos solo son visibles si el DLC Supporter Upgrade está comprado e instalado. Además, para empezar, asegúrate de que los colores (XYZ=RGB) estén, por ejemplo, entre 0.5 y 1.0, y ajústalos desde ahí. El valor W debe estar configurado en algo distinto de none.

## ¿El deslizador effect W no hace nada?

Hasta donde puedo ver, este valor no tiene efecto.

## ¿Qué son los otros valores de pintura en los archivos .yaml? ¿Puedo editarlos?

Todavía no los he revisado todos y no estoy seguro de qué hacen, si es que hacen algo. Actualmente el mod solo admite colores/materiales principal/secundario/terciario y los efectos.

## ¿Hay alguna forma de eliminar pinturas?

No desde dentro del mod por ahora. Pero puedes eliminarlas manualmente desde AppData/Local/PhantomBrigade/ModSavedData/LiveryGUI/*.yaml.

Esto debería ser seguro, salvo que las piezas de tus Mecas usarán la pintura predeterminada hasta que reasignes pinturas específicamente a las piezas afectadas.

## ¿Algo salió mal?

Puede haber información adicional en C:\Users\yourUserId\AppData\LocalLow\Brace Yourself Games\Phantom Brigade\Player.log.

En particular, busca mensajes de registro [LiveryGUI], o mensajes de registro sobre problemas en archivos "LiveryGUI".