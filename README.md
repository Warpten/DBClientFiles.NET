# DBClientFiles.NET
The new version of DBFilesClient.NET.

## Status

* Fix CodeGenerator.Clone to handle deep copying instead of shallow.
* Implement recursive calls to CodeGenerator so that users can more easily integrate complex types into their definitions (`C3Vector`, etc).
* Optimize all this shit.

## Preliminary performance tests

| File name                                     | Avg             | Best            | Worst                |
| --------------------------------------------- | --------------- | --------------- | -------------------- |
| AchievementEntry WDBC[1030 entries]           | 00.005937       | 00.005119       | 00.033047            |
| AreaTriggerEntry WDBC[964 entries]            | 00.001591       | 00.001430       | 00.003921            |
| AuctionHouseEntry WDBC[7 entries]             | 00.000936       | 00.000780       | 00.011687            |
| BankBagSlotPricesEntry WDBC[12 entries]       | 00.000440       | 00.000387       | 00.001360            |

| File name                                     | Avg             | Best            | Worst                |
| --------------------------------------------- | --------------- | --------------- | -------------------- |
| AchievementCategoryEntry WDBC[107 entries]    | 00.000696       | 00.000611       | 00.001332            |
| AchievementCriteriaEntry WDBC[10755 entries]  | 00.017214       | 00.014696       | 00.023326            |
| AchievementEntry WDBC[2645 entries]           | 00.008194       | 00.007441       | 00.012407            |
| AreaGroupEntry WDBC[830 entries]              | 00.001066       | 00.000909       | 00.002172            |
| AreaPOIEntry WDBC[889 entries]                | 00.002668       | 00.002317       | 00.007107            |
| AreaTableEntry WDBC[3206 entries]             | 00.007302       | 00.006655       | 00.009374            |
| ItemEntry WDB2[64775 entries]                 | 00.037305       | 00.034746       | 00.045401            |
| ItemSparseEntry WDB2[54086 entries]           | 00.299537       | 00.288375       | 00.363739            |

| File name                                     | Avg             | Best            | Worst                |
| --------------------------------------------- | --------------- | --------------- | -------------------- |
| AchievementEntry WDC1[5722 entries]           | 00.018454       | 00.016684       | 00.040712            |
| AreaTableEntry WDC1[5853 entries]             | 00.017130       | 00.015426       | 00.028891            |
| ItemSearchNameEntry WDC1[83614 entries]       | 00.151561       | 00.141429       | 00.186150            |

| File name                                     | Avg             | Best            | Worst                |
| --------------------------------------------- | --------------- | --------------- | -------------------- |
| AchievementEntry WDC2[6138 entries]           | 00.020611       | 00.018804       | 00.033515            |