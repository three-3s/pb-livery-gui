# Domande e risposte

## Quali lingue sono supportate?

La mod include traduzioni di base per tutte le lingue attualmente supportate dal gioco.

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
## Devo iniziare una nuova partita?

No. Questa mod dovrebbe supportare completamente i salvataggi esistenti.

## Posso ancora usare altre mod che aggiungono livree?

Sì. Non potrai sovrascriverle, ma puoi copiarle e modificare le copie.

## Dove vengono salvati i file delle livree?

AppData/Local/PhantomBrigade/ModSavedData/LiveryGUI/*.yaml

Per esempio:
C:/Users/yourUserID/AppData/Local/PhantomBrigade/ModSavedData/LiveryGUI

Puoi impacchettare questi file in una mod di livree.

## È possibile usare valori più grandi o più piccoli di quelli consentiti dagli slider?

Sì, ma al momento la mod non lo supporta direttamente. Almeno per i colori, i valori negativi rompono il colore e lo rendono nero. I colori troppo luminosi prima o poi rompono il colore e iniziano a mostrare porzioni nere sempre più grandi. I parametri W dei materiali possono causare grossi glitch di rendering, specialmente con certi valori XYZ degli effetti.

## Posso modificare una livrea integrata o una livrea fornita da altre mod?

Non direttamente, ma puoi salvare una copia della livrea e modificare la copia.

## Che succede se disinstallo questa mod?

Dovrebbe andare bene. Le livree che non esistono più appariranno come livree predefinite. Potresti però dover riassegnare le parti interessate ad altre livree, oppure fare clic destro nella pagina delle livree per svuotarle e impostarle su nessuna livrea. Se avevi una vecchia livrea del busto superiore che non esiste più, cambiare la livrea dell'intero Mech o dell'intero busto può lasciare il busto superiore bloccato sulla livrea predefinita finché non viene svuotato (clic destro).

A parte questo, il gioco sembra gestire abbastanza bene la disattivazione o la disinstallazione della mod.

## Vedo grandi cerchi bianchi?

L'intervallo normale dei valori è tra 0 e 1. Tutto il resto è sperimentale.

La causa principale del glitch di rendering dei grandi cerchi bianchi è un valore W del materiale primario, secondario o terzo troppo grande, positivo o negativo. A volte puoi aggirare il problema regolando lo slider XYZ dell'effetto corrispondente (per gli effetti: X=primario, Y=secondario, Z=terzo), oppure riducendo il colore RGB della parte problematica (rendendo R+G+B più piccolo). Ridurre RGB può renderlo più scuro, ma parte della luminosità può essere recuperata usando una componente A negativa per quel colore.

Nota: i grandi cerchi bianchi possono essere visibili solo da certi angoli, quindi usa i tasti di movimento della telecamera (WASD per impostazione predefinita sulle tastiere QWERTY) per esaminare il Mech da più angolazioni. Prova a superare leggermente il punto in cui i cerchi spariscono, così è meno probabile che riappaiano dal punto di vista giusto.

## Gli slider Supporter DLC non fanno nulla?

Gli effetti sono visibili solo se il DLC Supporter Upgrade è acquistato e installato. Inoltre, per iniziare, assicurati che i colori (XYZ=RGB) siano ad esempio tra 0.5 e 1.0, poi regolali da lì. Il valore W deve essere impostato su qualcosa di diverso da none.

## Lo slider effect W non fa nulla?

Per quanto posso vedere, questo valore non ha alcun effetto.

## Cosa sono gli altri valori delle livree nei file .yaml? Posso modificarli?

Non li ho ancora esaminati tutti e non sono sicuro di cosa facciano, se fanno qualcosa. La mod attualmente supporta solo colori/materiali primario/secondario/terzo e gli effetti.

## C è un modo per eliminare le livree?

Non dall'interno della mod, al momento. Ma puoi eliminarle manualmente da AppData/Local/PhantomBrigade/ModSavedData/LiveryGUI/*.yaml.

Questo dovrebbe essere sicuro, a parte il fatto che le parti dei tuoi Mech useranno la livrea predefinita finché non assegnerai di nuovo esplicitamente una livrea alle parti interessate.

## Qualcosa è andato storto?

Potrebbero esserci informazioni aggiuntive in C:\Users\yourUserId\AppData\LocalLow\Brace Yourself Games\Phantom Brigade\Player.log.

In particolare, cerca messaggi di log [LiveryGUI], oppure messaggi di log su problemi nei file "LiveryGUI".
