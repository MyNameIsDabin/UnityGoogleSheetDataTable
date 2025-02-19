# Tabby Sheet

A C# and Unity-compatible library that downloads Google Sheets and exports/imports them as binary assets.

## Tabby Sheet for Unity

![](https://github.com/MyNameIsDabin/UnityGoogleSheetDataTable/blob/main/Guide/02.png)

### Unity Demo

```cs
using TabbySheet;
using UnityEngine;

public class DemoScript : MonoBehaviour
{
    void Start()
    {
        TabbyDataSheet.Init();
        
        var foodsTable = DataSheet.Load<FoodsTable>();
        
        foreach (var data in foodsTable)
            Debug.Log(data.Name);
    }
}
```

### How to Import 
![](https://github.com/MyNameIsDabin/TabbySheet/blob/main/Guide/07.png)

URL : `https://github.com/MyNameIsDabin/TabbySheet.git?path=/TabbySheetUnity/Assets/TabbySheet`

Click the plus (+) button in the Unity Package Manager, select 'Add package from git URL...', enter the following URL, and then click the Add button to install the package.

## Ready

구글 API 접근을 위한 OAuth 클라이언트 JSON 파일과 몇가지 설정이 필요합니다. 

이 과정은 처음에 조금 번거롭게 느껴질 수 있지만 모든 프로젝트에서 실행되어야 하는 작업은 아닙니다.

**처음 한번만 설정해두면 다른 프로젝트에서는 추가적은 설정을 요구하지 않습니다.**

먼저, [구글 클라우드 플랫폼](https://cloud.google.com/free?utm_source=google&utm_medium=cpc&utm_campaign=japac-KR-all-ko-dr-BKWS-all-cloud-trial-EXA-dr-1605216&utm_content=text-ad-none-none-DEV_c-CRE_631197830139-ADGP_Hybrid%20%7C%20BKWS%20-%20EXA%20%7C%20Txt%20~%20GCP_General_google%20cloud%20platform_main-KWID_43700073965135596-kwd-528089444121&userloc_1009871-network_g&utm_term=KW_%EA%B5%AC%EA%B8%80%20%ED%81%B4%EB%9D%BC%EC%9A%B0%EB%93%9C%ED%94%8C%EB%9E%AB%ED%8F%BC&gclid=CjwKCAiAuaKfBhBtEiwAht6H73-rV5y9jwbLm-r-LY9qSXahtDxID_udznpAdZqQIgAhuJDnJv-jAxoCo88QAvD_BwE&gclsrc=aw.ds&hl=ko) 에서 프로젝트를 만들고 아래와 같이 사용자 인증 정보에서 OAuth 클라이언트 ID를 만듭니다.

이 때 애플리케이션 유형을 '데스크톱 앱'으로 설정 합니다.

<img src="https://github.com/MyNameIsDabin/UnityGoogleSheetDataTable/blob/main/Guide/03.png" width="750px"></img><br/>

여기서 생성된 정보창에서 JSON 파일을 다운로드 받습니다. 이 JSON 파일을 API 요구하는 'credential.json' 이라고 부르겠습니다.

<img src="https://github.com/MyNameIsDabin/UnityGoogleSheetDataTable/blob/main/Guide/04.png" width="450px"></img><br/>


Google Sheets API, Google Drive API 를 사용 설정됨 상태로 전환하면 준비가 끝납니다.

<img src="https://github.com/MyNameIsDabin/UnityGoogleSheetDataTable/blob/main/Guide/06.png" width="650px"></img><br/>

## 사용 방법

1. package 파일을 임포트 합니다.
2. 유니티 상단의 [Tools - TabbySheet] 혹은 Shift+T 를 눌러서 시트 툴을 에디터를 열어줍니다.
3. 좌측상단의 Edit 버튼을 눌러서 Google Sheet URL, credential.json 경로를 기입해줍니다.
4. `[Download Google Sheet]`를 눌러서 해당 URL 로 부터 시트 파일을 다운로드 받습니다.
5. `[Create Table Class]`를 눌러서 코드에서 테이블 데이터에 쉽게 접근 가능하게 해주는 싱글톤 클래스 파일을 생성합니다.
6. `[Export Binary]`를 눌러서 다운로드받은 엑셀 시트를 바이너리 파일로 변환합니다. (이거까지 눌러야 싱글톤 클래스로 바이너리 데이터를 불러와서 접근 가능)

끝! 이제 코드에서 싱글톤으로 접근할 수 있습니다.

## 엑셀 테이블 규칙

사용하려는 데이터 테이블 엑셀은 다음 헤더의 규칙을 지켜야 합니다.

![image](https://user-images.githubusercontent.com/26871928/218316540-ae3a2602-af48-4f31-b8a7-72a1588272e8.png)

첫번째 라인 : 헤더의 설명 (어떤 텍스트가 들어가도 상관 없습니다) <br></br>
두번째 라인 : 데이터의 변수로 활용될 이름 <br></br>
세번째 라인 : 자료형 **int, string, float, double, bool, 사용자 지정 Enum 타입**을 지원합니다. <br></br>
네번째 라인 : 옵션입니다. 현재는 UniqueKey만 사용 가능하며, 코드상에서 `GetDataBy[이름]` 으로 접근할 수 있게 됩니다. <br></br>
다섯번째 라인 이후 부터는 실제 데이터가 정의되면 됩니다. <br></br>

----------------------------------------
Project Copyright and License
----------------------------------------
© 2025 Davin

This software is licensed under the Apache License 2.0.
For more information, please refer to the LICENSE file.

----------------------------------------
Third-Party Dependencies
----------------------------------------

[Apache-2.0 license](https://github.com/googleapis/google-api-nodejs-client/blob/main/LICENSE)
- Google API Client Libraries (Auth, Drive.v3, Sheets.v4)

[MIT license](https://licenses.nuget.org/MIT)
- [ExcelDataReader](https://www.nuget.org/packages/exceldatareader/)
