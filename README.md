# GoogleSheet DataTable for Unity

![가이드2](https://github.com/MyNameIsDabin/UnityGoogleSheetDataTable/blob/main/Guide/02.png)

구글 스프레드시트를 불러와서 사용 가능한 초간단 데이터 테이블 에디터 툴.

스크립트 몇개만 추가하면 사용 가능합니다..!

## Quick Start

1. package 파일을 임포트 합니다.
2. 유니티 상단의 [Tools - DataTable] 혹은 Shift+T 를 눌러서 시트 툴을 에디터를 열어줍니다.
3. 좌측상단의 Edit 버튼을 눌러서 Google Sheet URL, Credential.json 경로를 기입해줍니다.
4. `[Download Google Sheet]`를 눌러서 해당 URL 로 부터 시트 파일을 다운로드 받습니다.
5. `[Create Table Class]`를 눌러서 코드에서 테이블 데이터에 쉽게 접근 가능하게 해주는 싱글톤 클래스 파일을 생성합니다.
6. `[Export Binary]`를 눌러서 다운로드받은 엑셀 시트를 바이너리 파일로 변환합니다. (이거까지 눌러야 싱글톤 클래스로 바이너리 데이터를 불러와서 접근 가능)

끝! 이제 코드에서 다음과 같이 테이블별 싱글톤으로 접근합니다.
```
// Access by Unique Key
var firstFood = FoodsTable.Instance.GetDataById(0); 

Debug.Log(firstFood.Name);
Debug.Log(firstFood.CanBuy);
```
