# 問題與回答

## 支援哪些語言？

本模組為遊戲目前支援的所有語言提供基礎翻譯。

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

## 我需要開始新遊戲嗎？

不需要。本模組應可完整支援現有存檔。

## 我還能使用其他新增塗裝的模組嗎？

可以。你不能覆寫它們，但可以複製它們並編輯副本。

## 塗裝檔案儲存在哪裡？

AppData/Local/PhantomBrigade/ModSavedData/LiveryGUI/*.yaml

例如：
C:/Users/yourUserID/AppData/Local/PhantomBrigade/ModSavedData/LiveryGUI

你可以將這些檔案打包成一個塗裝模組。

## 是否可以使用比滑桿允許範圍更大或更小的值？

可以，但模組目前不支援直接這樣做。至少對顏色而言，負值會「破壞」顏色並使其變黑。過亮的顏色最終也會「破壞」顏色，並開始顯示越來越大的黑色區域。材質 W 參數可能觸發一些嚴重的渲染故障，尤其是在某些效果 XYZ 值下。

## 我可以修改內建塗裝或其他模組提供的塗裝嗎？

不能直接修改，但你可以儲存該塗裝的副本，然後編輯這個副本。

## 如果我解除安裝這個模組會怎樣？

應該沒有問題。不再存在的塗裝會看起來像預設塗裝。但你可能需要重新指定受影響部件使用其他塗裝，或者在塗裝頁面中按右鍵，將塗裝清除為「無塗裝」。如果你有一個舊的「上軀幹」塗裝已不存在，改變整台機甲或整個軀幹的塗裝時，可能會讓上軀幹卡在「預設塗裝」，直到它被清除（右鍵）。

除此之外，遊戲似乎能相當好地處理停用或解除安裝本模組的情況。

## 我看到了很大的白色圓圈？

數值的正常範圍是 0 到 1。其他數值都屬於實驗性用法。

造成「巨大白色圓圈」渲染故障的主要原因，是主色、副色或第三色的材質 W 值過大，正負都可能發生。你有時可以透過調整對應的效果 XYZ 滑桿來規避這個問題（效果中 X=主色，Y=副色，Z=第三色），或降低問題部件的 RGB 顏色（讓 R+G+B 更小）。降低 RGB 可能會讓顏色變暗，但可以透過為該顏色使用負的 A 分量來恢復一部分亮度。

注意：巨大白色圓圈可能只會從某些角度可見，所以請使用「移動攝影機」按鍵（QWERTY 鍵盤預設 WASD）從多個角度檢查機甲。嘗試比剛好消除白圈再多調一點，以協助確保它們不會在某個剛好的角度再次出現。

## 「Supporter DLC」滑桿沒有任何效果？

這些效果只有在已購買並安裝 Supporter Upgrade DLC 時才可見。另外，開始調整時請先確保顏色（XYZ=RGB）大約在 0.5 到 1.0 之間，然後再從那裡繼續調整。W 值應設定為「none」之外的選項。

## 「effect W」滑桿沒有任何效果？

就我所能判斷，這個值沒有作用。

## 塗裝 .yaml 檔案中的其他塗裝值是什麼？我可以編輯它們嗎？

我還沒有研究完所有這些值，也不確定它們是否有什麼作用。模組目前只支援主色/副色/第三色的顏色和材質，以及效果參數。

## 有沒有辦法刪除塗裝？

目前不能在模組內部刪除。但你可以手動從 AppData/Local/PhantomBrigade/ModSavedData/LiveryGUI/*.yaml 刪除任何塗裝檔案。

這應該是安全的，只是你的機甲部件會使用預設塗裝，直到你專門為受影響的機甲部件重新指定塗裝。

## 出問題了？

C:\Users\yourUserId\AppData\LocalLow\Brace Yourself Games\Phantom Brigade\Player.log 中可能有更多資訊。

請特別尋找 [LiveryGUI] 記錄訊息，或關於 “LiveryGUI” 檔案問題的記錄訊息。