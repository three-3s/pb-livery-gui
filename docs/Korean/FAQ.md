# 질문과 답변

## 어떤 언어를 지원하나요?

이 모드는 현재 게임이 지원하는 모든 언어에 대한 기본 번역을 포함합니다.

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
## 새 게임을 시작해야 하나요?

아니요. 이 모드는 기존 저장 파일을 완전히 지원할 것입니다.

## 도색을 추가하는 다른 모드도 계속 사용할 수 있나요?

네. 덮어쓸 수는 없지만, 복사한 뒤 그 복사본을 편집할 수 있습니다.

## 도색 파일은 어디에 저장되나요?

AppData/Local/PhantomBrigade/ModSavedData/LiveryGUI/*.yaml

예시:
C:/Users/yourUserID/AppData/Local/PhantomBrigade/ModSavedData/LiveryGUI

이 파일들을 도색 모드로 묶어 배포해도 됩니다.

## 슬라이더가 허용하는 범위보다 크거나 작은 값을 사용할 수 있나요?

가능하지만, 현재 모드는 이를 직접 지원하지 않습니다. 적어도 색상의 경우 음수 값은 색상을 깨뜨려 검게 만듭니다. 너무 밝은 색상도 결국 색상이 깨지며 점점 더 큰 검은 영역을 표시하기 시작합니다. 재질 W 매개변수는 특히 특정 효과 XYZ 값과 함께 사용할 때 큰 렌더링 오류를 일으킬 수 있습니다.

## 기본 도색이나 다른 모드가 제공하는 도색을 바꿀 수 있나요?

직접 바꿀 수는 없지만, 해당 도색의 복사본을 저장하고 그 복사본을 편집할 수 있습니다.

## 이 모드를 제거하면 어떻게 되나요?

대체로 괜찮을 것입니다. 더 이상 존재하지 않는 도색은 기본 도색처럼 보입니다. 다만 영향을 받은 부품에 다른 도색을 다시 지정하거나, 도색 페이지에서 오른쪽 클릭으로 도색을 없음 상태로 지워야 할 수 있습니다. 더 이상 존재하지 않는 예전 상부 몸통 도색이 있었다면, 전체 메카나 전체 몸통의 도색을 바꿀 때 상부 몸통이 지워질 때까지 기본 도색에 고정된 것처럼 남을 수 있습니다(오른쪽 클릭).

그 외에는 게임이 모드 비활성화나 제거를 충분히 잘 처리하는 것으로 보입니다.

## 큰 흰색 원이 보입니다.

값의 일반적인 범위는 0에서 1 사이입니다. 그 밖의 값은 실험적입니다.

큰 흰색 원 렌더링 오류의 주요 원인은 주/보조/제3 재질 W 값이 너무 큰 경우입니다. 양수와 음수 모두에서 발생할 수 있습니다. 대응하는 효과 XYZ 슬라이더를 조정해 우회할 수 있는 경우가 있습니다(효과에서는 X=주, Y=보조, Z=제3). 또는 문제가 되는 부품의 RGB 색상을 낮춰 보세요(R+G+B를 더 작게). RGB를 낮추면 어두워질 수 있지만, 해당 색상의 A 성분을 음수로 사용하면 밝기를 일부 되돌릴 수 있습니다.

참고: 큰 흰색 원은 특정 각도에서만 보일 수 있으므로 카메라 이동 키(QWERTY 키보드 기본값은 WASD)를 사용해 여러 각도에서 메카를 확인하세요. 흰 원이 겨우 사라지는 지점보다 조금 더 여유 있게 조정하면, 딱 맞는 각도에서 다시 나타나는 일을 줄일 수 있습니다.

## Supporter DLC 슬라이더가 아무 효과도 없습니다.

이 효과는 Supporter Upgrade DLC를 구매하고 설치한 경우에만 보입니다. 또한 처음에는 색상(XYZ=RGB)을 예를 들어 0.5에서 1.0 사이로 두고 거기서부터 조정하세요. W 값은 none이 아닌 값으로 설정해야 합니다.

## effect W 슬라이더가 아무 효과도 없습니다.

제가 확인한 바로는 이 값에는 효과가 없습니다.

## 도색 .yaml 파일의 다른 값들은 무엇인가요? 편집해도 되나요?

아직 모두 살펴보지 않았고, 실제로 무엇을 하는지 또는 어떤 역할이 있는지도 확실하지 않습니다. 현재 모드는 주/보조/제3 색상과 재질, 그리고 효과만 지원합니다.

## 도색을 삭제하는 방법이 있나요?

현재는 모드 안에서는 삭제할 수 없습니다. 하지만 AppData/Local/PhantomBrigade/ModSavedData/LiveryGUI/*.yaml 에서 수동으로 원하는 파일을 삭제할 수 있습니다.

이는 안전할 것입니다. 다만 영향을 받은 메카 부품에 다시 도색을 명시적으로 지정할 때까지 그 부품은 기본 도색을 사용합니다.

## 문제가 생겼나요?

C:\Users\yourUserId\AppData\LocalLow\Brace Yourself Games\Phantom Brigade\Player.log 에 추가 정보가 있을 수 있습니다.

특히 [LiveryGUI] 로그 메시지나 “LiveryGUI” 파일 문제에 대한 로그 메시지를 찾아보세요.