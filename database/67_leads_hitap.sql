/* ==========================================================================
   67) LEAD MERKEZI: HITAP (Ad Bey / Ad Hanim)
   Gender     : E (erkek) / K (kadin) / NULL (henuz belirlenmedi)
   GenderSure : 1 = isim listesinden ya da elle kesin; 0 = tahmin (onay kuyrugunda "kontrol edin")
   ========================================================================== */
IF COL_LENGTH('dbo.LeadContacts', 'Gender') IS NULL
    ALTER TABLE dbo.LeadContacts ADD Gender NCHAR(1) NULL;
GO
IF COL_LENGTH('dbo.LeadContacts', 'GenderSure') IS NULL
    ALTER TABLE dbo.LeadContacts ADD GenderSure BIT NOT NULL CONSTRAINT DF_LeadContacts_GenderSure DEFAULT(0);
GO
