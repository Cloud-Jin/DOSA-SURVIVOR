# DOSA-SURVIVOR 소개
## 1. 소개
![Image](https://github.com/user-attachments/assets/483a4e2c-24b6-41cd-b0e3-57596e13aeec)

<div align="center"> < 게임 플레이 사진 > </div>

- Unity 3D 로그라이크 게임입니다.
- AOS / iOS 동시출시
- 형상관리: Git SourceTree

## 2. 개발환경
- Unity 2022.3.14f1 LTS
- C#
- Xcode 16.2
- Elixir

## 3. 사용기술
|기술|설명|
|------|---
|디자인 패턴|● 싱글톤 패턴을 사용하여 Manager 관리 <br> ● FSM으로 유닛 상태관리 <br> ● MVRP 패턴으로 작성 <br> ● BlackBoard 전투 시스템 데이터 관리| 
|Object Pooling|자주 사용되는 객체는 Pool 관리하여 재사용 (Projectile, Unit, Effect)| 
|UI| Unirx 사용으로 데이터에 따라 UI 수정|
|Data| 엑셀 파싱으로 S3 스토리지 Addressable 로 업데이트 관리|
|API| Elixir 로 소켓통신| 

## 4. 구현
- Objec
  - 플레이어블 유닛
    - 9 종 영웅
  - 몬스터
    - 100종 (근거리, 원거리, 특수공격, 특수패턴)
  - 보스
    - 30 종의 패턴
  - 인게임 아이템
    - 물약, 자석, 폭탄, 경험치
  - Area Effector
    - 직선, 원, 포물선, 박스 

- 데이터 패치 시스템
  -  Addressable로 관리
    - 에셋, 사운드, 데이터등 업데이트시 감지하여 자동 업데이트 가능
    - 데이터 버전 유지 기능
    - 강제 업데이트 기능
 
- 젠킨스를 활용한 CI/CD 구현
