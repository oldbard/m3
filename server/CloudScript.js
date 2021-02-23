// Declarations

const defaultCatalog = '1';
const GEM_CURRENCY_CODE = 'HC';
const GOLD_CURRENCY_CODE = 'SC';
const UPGRADABLE_LEVEL = 'UpgradeDurationLevel';
const UPGRADABLE_TIMESTAMP = 'UpgradeDurationTimestamp';
const UPGRADABLE_MULTIPLIER = 'UpgradeMultiplier';
const UPGRADABLE_INITIAL_DURATION = 'InitialValue';
const CATALOG_ITEM = "Timer";


// Handlers

handlers.StartDurationUpgrade = StartDurationUpgrade;
handlers.SkipDurationUpgrade = SkipDurationUpgrade;
handlers.TryFinishUpgradingDuration = TryFinishUpgradingDuration;


// Client Calls

function StartDurationUpgrade() {
    var upgradeDuration = GetUpgradeDurationItem();
    var initialCost = GetUpgradeDurationItemCost(upgradeDuration);
    var increasePerLevel = GetUpgradeDurationItemUpgradeMultiplier(upgradeDuration);

    var currentLevel = 1;

    var userData = GetUserData();

    if (userData.hasOwnProperty(UPGRADABLE_LEVEL)) {
        currentLevel = Number(JSON.parse(userData[UPGRADABLE_LEVEL].Value));
    }

    var upgradeCost = CurrentDurationUpgradeCost(initialCost, currentLevel, increasePerLevel);

    var availableSC = GetAvailableSC();

    log.info("Player SC: " + availableSC + ", increase per level: " + increasePerLevel);
    if (availableSC >= upgradeCost) {
        log.info("Got enough sc to do upgrade.");

        SubtractUserVirtualCurrency(upgradeCost, GOLD_CURRENCY_CODE);

        SetUpgradeData(currentLevel);

        return true;
    }

     log.info("Not enough sc to do upgrade.");
     return false;
}

function SkipDurationUpgrade() {
    var upgradeDuration = GetUpgradeDurationItem();
    var initialCost = GetUpgradeDurationItemSkipCost(upgradeDuration);
    var increasePerLevel = GetUpgradeDurationItemUpgradeMultiplier(upgradeDuration);

    var userData = GetUserData();
    var currentLevel = Number(JSON.parse(userData[UPGRADABLE_LEVEL].Value));

    var skipUpgradeCost = CurrentDurationUpgradeCost(initialCost, currentLevel, increasePerLevel);

    var availableHC = GetAvailableHC();

    log.info("Player HC: " + availableHC + ", increase per level: " + increasePerLevel);
    if (availableHC >= skipUpgradeCost) {
        log.info("Got enough hc to do upgrade.");

        SubtractUserVirtualCurrency(skipUpgradeCost, GEM_CURRENCY_CODE);

        SetUpgradeDataFinished(currentLevel + 1);

        return true;
    }

    log.info("Not enough hc to do upgrade.");
    return false;
}

function TryFinishUpgradingDuration() {
    var userData = GetUserData();

    var initialTimestamp = Number(JSON.parse(userData[UPGRADABLE_TIMESTAMP].Value));
    if (initialTimestamp < 1) {
        log.info("Not upgrading!");

        return true;
    }
    var currentLevel = Number(JSON.parse(userData[UPGRADABLE_LEVEL].Value));

    var upgradeDuration = GetUpgradeDurationItem();
    var increasePerLevel = Number(GetUpgradeDurationItemUpgradeMultiplier(upgradeDuration));
    var initialDuration = Number(GetUpgradeDurationItemInitialDuration(upgradeDuration));

    var duration = Number(CurrentUpgradeDuration(initialDuration, currentLevel, increasePerLevel));

    var timestamp = Number(GetServerTimestamp());

    if (initialTimestamp + duration <= timestamp) {
        SetUpgradeDataFinished(currentLevel + 1);

        return true;
    }

    log.info("Time Left: " + initialTimestamp + duration - timestamp);

    return false;
}


// Get Data Calls

function GetUserData() {
    var getUserDataRequest = {
        PlayFabId: currentPlayerId,
        Keys: {
            UPGRADABLE_LEVEL, UPGRADABLE_TIMESTAMP
        }
    };
    
    var getUserDataResponse = server.GetUserReadOnlyData(getUserDataRequest);

    if (!getUserDataResponse.Data.hasOwnProperty(UPGRADABLE_LEVEL)) {
        log.info(UPGRADABLE_LEVEL + " not found!");
    }

    return getUserDataResponse.Data;
}

function GetAvailableSC() {
    var inventory = server.GetUserInventory({ PlayFabId: currentPlayerId });

    if (!inventory) {
        log.error("Inventory not found!");
        throw "Inventory not found!";
    }

    if (!inventory.VirtualCurrency) {
        log.error("Inventory VCs not found!");
        throw "Inventory VCs not found!";
    }

    return inventory.VirtualCurrency[GOLD_CURRENCY_CODE];
}

function GetAvailableHC() {
    var inventory = server.GetUserInventory({ PlayFabId: currentPlayerId });

    if (!inventory) {
        log.error("Inventory not found!");
        throw "Inventory not found!";
    }

    if (!inventory.VirtualCurrency) {
        log.error("Inventory VCs not found!");
        throw "Inventory VCs not found!";
    }

    return inventory.VirtualCurrency[GEM_CURRENCY_CODE];
}


// Set Data Calls

function SetUpgradeData(level) {
    var timestamp = GetServerTimestamp();

    var updateUserReadOnlyDataRequest = {
        PlayFabId: currentPlayerId,
        Data: {}
    };
    updateUserReadOnlyDataRequest.Data[UPGRADABLE_LEVEL] = JSON.stringify(level);
    updateUserReadOnlyDataRequest.Data[UPGRADABLE_TIMESTAMP] = JSON.stringify(timestamp);
    
    server.UpdateUserReadOnlyData(updateUserReadOnlyDataRequest);    
}

function SetUpgradeDataFinished(level) {
    var timestamp = GetServerTimestamp();

    var updateUserReadOnlyDataRequest = {
        PlayFabId: currentPlayerId,
        Data: {}
    };
    updateUserReadOnlyDataRequest.Data[UPGRADABLE_LEVEL] = JSON.stringify(level);
    updateUserReadOnlyDataRequest.Data[UPGRADABLE_TIMESTAMP] = JSON.stringify(0);
    
    server.UpdateUserReadOnlyData(updateUserReadOnlyDataRequest);    
}

function SubtractUserVirtualCurrency(amount, currencyType) {
    var subtractUserVirtualCurrencyRequest = {
	    "PlayFabId" : currentPlayerId,
	    "VirtualCurrency": currencyType,
	    "Amount": amount
    };

    var subtractUserVirtualCurrencyResult = server.SubtractUserVirtualCurrency(subtractUserVirtualCurrencyRequest);
    log.info(subtractUserVirtualCurrencyResult);
}


// Utils

function CurrentDurationUpgradeCost(initialCost, currentLevel, increasePerLevel) {

    var cost = initialCost;

    var i;
    for (i = 1; i < currentLevel; i++) {
        cost *= increasePerLevel;
    }

    log.info("Cost: " + cost);

    return cost;
}

function CurrentUpgradeDuration(initialUpgradeDuration, currentLevel, increasePerLevel) {

    var duration = initialUpgradeDuration;

    var i;
    for (i = 1; i < currentLevel; i++) {
        duration *= increasePerLevel;
    }

    log.info("duration: " + duration);

    return duration;
}

function GetUpgradeDurationItem() {
    var catalog = server.GetCatalogItems({ CatalogVersion: defaultCatalog });

    var upgradeDuration;

    for (var catalogItem in catalog.Catalog) {
        var item = catalog.Catalog[catalogItem];
        
        if(item.ItemId == CATALOG_ITEM) {
            upgradeDuration = item;
        }
    }

    if (!upgradeDuration) {
        log.error("Catalog Item not found!");
        throw "Catalog Item not found!";
    }

    return upgradeDuration;
}

function GetUpgradeDurationItemCost(upgradeDuration) {
    if (!upgradeDuration.VirtualCurrencyPrices) {
        log.error("Catalog Item VCs not found!");
        throw "Catalog Item VCs not found!";
    }

    return upgradeDuration.VirtualCurrencyPrices[GOLD_CURRENCY_CODE];
}

function GetUpgradeDurationItemSkipCost(upgradeDuration) {
    if (!upgradeDuration.VirtualCurrencyPrices) {
        log.error("Catalog Item VCs not found!");
        throw "Catalog Item VCs not found!";
    }

    return upgradeDuration.VirtualCurrencyPrices[GEM_CURRENCY_CODE];
}

function GetUpgradeDurationItemUpgradeMultiplier(upgradeDuration) {
    var catalogItemCustomData = JSON.parse(upgradeDuration.CustomData);

    if (!catalogItemCustomData.hasOwnProperty(UPGRADABLE_MULTIPLIER)) {
        log.error(UPGRADABLE_MULTIPLIER + " not found!");
        throw UPGRADABLE_MULTIPLIER + " not found!";
    }

    return catalogItemCustomData[UPGRADABLE_MULTIPLIER];
}

function GetUpgradeDurationItemInitialDuration(upgradeDuration) {
    var catalogItemCustomData = JSON.parse(upgradeDuration.CustomData);

    if (!catalogItemCustomData.hasOwnProperty(UPGRADABLE_INITIAL_DURATION)) {
        log.error(UPGRADABLE_INITIAL_DURATION + " not found!");
        throw UPGRADABLE_INITIAL_DURATION + " not found!";
    }

    return catalogItemCustomData[UPGRADABLE_INITIAL_DURATION];
}

function GetServerTimestamp() {
    var now = new Date();
    var time = now.getTime(); // miliseconds

    //  Get timestamp in seconds
    time = Math.floor(time / 1000); // cast to seconds
    
    return time;
}
