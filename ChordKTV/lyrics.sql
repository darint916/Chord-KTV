UPDATE "Songs" s 
SET "LrcLyrics" = '[00:00.00]오빤 강남 스타일
[00:02.00]강남 스타일
[00:10.00]낮에는 따사로운 인간적인 여자
[00:15.00]커피 한잔의 여유를 아는 품격 있는 여자
[00:20.00]밤이 오면 심장이 뜨거워지는 여자
[00:25.00]그런 반전 있는 여자
[00:30.00]나는 사나이
[00:35.00]낮에는 너만큼 따사로운 그런 사나이
[00:40.00]커피 식기도 전에 원샷 때리는 사나이
[00:45.00]밤이 오면 심장이 터져 버리는 사나이
[00:50.00]그런 사나이'
FROM "GeniusMetaData" gm 
WHERE s."GeniusMetaDataGeniusId" = gm."GeniusId" 
AND gm."GeniusId" = 3756527;