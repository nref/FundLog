window.plaidCreate = (token, callback) => {

    if (!("Plaid" in window))
    {
      console.log("Cannot load Plaid. Its js script is not loaded. Check for script blockers e.g. NoScript or uMatrix.");
      return;
    }

    const handler = Plaid.create({
        token: token,
        onSuccess: (public_token, metadata) => {
            console.log(`onSuccess(public_token = ${public_token}, metadata = ${JSON.stringify(metadata, null, 2)})`)
            callback.invokeMethodAsync("Run", {
                linkToken: token,
                publicToken: public_token,
            });
            callback.invokeMethodAsync("Dispose");
        },
        onLoad: () => {
            console.log("Loaded Plaid");

            // For testing: Fake a callback.
            //callback.invokeMethodAsync("Run", {
            //    linkToken: "link_token",
            //    publicToken: "public_token",
            //});
            //callback.invokeMethodAsync("Dispose");
        },
        onExit: (err, metadata) => {
            console.log(`Plaid exited. err = ${JSON.stringify(err, null, 2)}, metadata = ${JSON.stringify(metadata, null, 2)}`)
        },
        onEvent: (eventName, metadata) => {
            console.log(`Plaid event: ${eventName}, metadata = ${JSON.stringify(metadata, null, 2)}`)
        },
        receivedRedirectUri: null,
    });

    handler.open()
}