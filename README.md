# DBClientFiles.NET
The new version of DBFilesClient.NET.

## Status

* Fix CodeGenerator.Clone to handle deep copying instead of shallow.
* Implement recursive calls to CodeGenerator so that users can more easily integrate complex types into their definitions (`C3Vector`, etc).
* Optimize all this shit.

## Preliminary performance tests

| File name                                     | Avg             | Best            | Worst                |
| --------------------------------------------- | --------------- | --------------- | -------------------- |
| AchievementEntry WDBC[1030 entries]           | 00.007127       | 00.005467       | 00.013819            |
| AreaTriggerEntry WDBC[964 entries]            | 00.002541       | 00.001591       | 00.016084            |
| AuctionHouseEntry WDBC[7 entries]             | 00.001661       | 00.000860       | 00.012985            |
| BankBagSlotPricesEntry WDBC[12 entries]       | 00.000610       | 00.000417       | 00.001413            |

| File name                                     | Avg             | Best            | Worst                |
| --------------------------------------------- | --------------- | --------------- | -------------------- |
| AchievementCategoryEntry WDBC[107 entries]    | 00.001429       | 00.000678       | 00.010619            |
| AchievementCriteriaEntry WDBC[10755 entries]  | 00.024117       | 00.018183       | 00.052164            |
| AchievementEntry WDBC[2645 entries]           | 00.010966       | 00.008248       | 00.026311            |
| AreaGroupEntry WDBC[830 entries]              | 00.001637       | 00.001059       | 00.008410            |
| AreaPOIEntry WDBC[889 entries]                | 00.003513       | 00.002570       | 00.014765            |
| AreaTableEntry WDBC[3206 entries]             | 00.009130       | 00.007369       | 00.019163            |
| ItemEntry WDB2[64775 entries]                 | 00.061222       | 00.048931       | 00.095662            |
| ItemSparseEntry WDB2[54086 entries] :exclamation:           | 00.425409       | 00.387373       | 00.526543            |

| File name                                     | Avg             | Best            | Worst                |
| --------------------------------------------- | --------------- | --------------- | -------------------- |
| AchievementEntry WDC1[5722 entries]           | 00.024973       | 00.020606       | 00.070128            |
| AreaTableEntry WDC1[5853 entries]             | 00.020753       | 00.016604       | 00.055182            |
| ItemSearchNameEntry WDC1[83614 entries]       | 00.195428       | 00.166773       | 00.261535            |

| File name                                     | Avg             | Best            | Worst                |
| --------------------------------------------- | --------------- | --------------- | -------------------- |
| AchievementEntry WDC2[6138 entries]           | 00.028677       | 00.023751       | 00.054449            |
