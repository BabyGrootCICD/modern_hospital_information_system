# SHIS.HisOrder(SourceCode)


## MOHD DB Server:
1. Create Database `MOHD`.
2. Restore DB using `MOHD-DB-BK-20231211-2.sql`.
3. Verify if the mohd_users gateway credentials are consistent across different hospitals.

## MOHD Web API:
1. Check `Models -> MOHDContext.cs` for correct PostgreSQL connection string.
2. Follow deployment and publishing process similar to `SHIS.HisOrder`.

## HIS Gateway:
1. Ensure Table Schema matches project requirements (Refer: `HIS-Gatweay-Table-Schema-20231211.sql`).
2. Confirm `shis_users` gateway credentials match those in `MOHD`.
3. Verify `Models -> GatewayContext.cs` for correct PostgreSQL connection string.
4. Confirm deployment environment has .NET CORE 6 or above installed.

### HIS Gateway Offline Feature Version 1.0.0.2
1. Please copy the content format of appsettings.DefaultSample.json to appsettings.json and publish it to each hospital environment.
2. For the MOHD Server environment, please copy the content format of appsettings.MOHDSample.json to appsettings.json and publish it.
3. Ensure Table Schema matches project requirements (Refer: `HIS-Gateway-Ver-1.0.0.2.sql`).
4. Please use the export function in each hospital environment and copy the generated .txt files under the SHIS_export folder in the project directory to the MOHD Server.
5. The import function can only be used on the MOHD Server. When importing, please place the .txt files under the SHIS_import folder in the project directory.

## MOHD DB Server:
1. 建立名為 `MOHD` 的資料庫。
2. 使用 `MOHD-DB-BK-20231211-2.sql` 還原資料庫。
3. 確認 `mohd_users` Gateway 的帳號密碼是否與各院區一致。

## MOHD Web API:
1. 確認 `Models -> MOHDContext.cs` 中的 PostgreSQL 連線字串是否正確。
2. 遵循與 `SHIS.HisOrder` 相似的部署和發佈流程。

## HIS Gateway:
1. 確保資料表結構符合專案需求（參考：`HIS-Gatweay-Table-Schema-20231211.sql`）。
2. 確認 `shis_users` Gateway 的帳號密碼與 `MOHD` 中的一致。
3. 確認 `Models -> GatewayContext.cs` 中的 PostgreSQL 連線字串是否正確。
4. 確認部署環境已安裝 .NET CORE 6 或以上版本。

###  HIS Gateway 離線功能 Version 1.0.0.2
1. 各醫院環境務請將appsettings.DefaultSample.json 內容格式複製到 appsettings.json 後發佈.
2. MOHD Server環境請將appsettings.MOHDSample.json 內容格式複製到 appsettings.json 後發佈.
3. 確保資料表結構符合專案需求 (Refer: `HIS-Gateway-Ver-1.0.0.2.sql`).
4. 請於各醫院環境下使用匯出功能，並將專案目錄下SHIS_export資料夾底下所產生.txt檔案，複製到MOHD Server.
5. 匯入功能只能於MOHD Sever上使用，匯入時請將.txt檔案放置專案目錄下SHIS_import資料夾底下.

