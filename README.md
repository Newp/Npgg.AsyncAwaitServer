# Npgg.AsyncAwaitServer
.net core 의 async/await 서버


# 성능
Async/Await 과 AsyncCallback (Begin/End) 의 성능차이는 거의 없음.


# 많이 단순해진 처리

async callback 방식 기준으로 항상 2개씩 콜백을 선언해야 했고

header/ body를 나누어서 받던 코드라면 더 복잡해졌다.

또한 callback 구조이기 때문에 함수 호출되는 과정들이 다소 복잡한편이었다.


하지만 async/await 방식은 순서대로 처리되기 때문에

callback 구조에서 볼 수 없는 단순한 코드흐름을 가지고 있기 때문에 매우 쉽다.


# ArrayPool<T>
  
  

