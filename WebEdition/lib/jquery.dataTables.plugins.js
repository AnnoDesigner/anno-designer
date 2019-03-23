$.fn.dataTableExt.oApi.fnReloadAjax = function ( oSettings, sNewSource, fnCallback, bStandingRedraw )
{
    if ( typeof sNewSource != 'undefined' && sNewSource != null ) {
        oSettings.sAjaxSource = sNewSource;
    }

    // Server-side processing should just call fnDraw
    if ( oSettings.oFeatures.bServerSide ) {
        this.fnDraw();
        return;
    }

    this.oApi._fnProcessingDisplay( oSettings, true );
    var that = this;
    var iStart = oSettings._iDisplayStart;
    var aData = [];

    this.oApi._fnServerParams( oSettings, aData );

    oSettings.fnServerData.call( oSettings.oInstance, oSettings.sAjaxSource, aData, function(json) {
        /* Clear the old information from the table */
        that.oApi._fnClearTable( oSettings );

        /* Got the data - add it to the table */
        var aData =  (oSettings.sAjaxDataProp !== "") ?
            that.oApi._fnGetObjectDataFn( oSettings.sAjaxDataProp )( json ) : json;

        for ( var i=0 ; i<aData.length ; i++ )
        {
            that.oApi._fnAddData( oSettings, aData[i] );
        }

        oSettings.aiDisplay = oSettings.aiDisplayMaster.slice();

        if ( typeof bStandingRedraw != 'undefined' && bStandingRedraw === true )
        {
            oSettings._iDisplayStart = iStart;
            that.fnDraw( false );
        }
        else
        {
            that.fnDraw();
        }

        that.oApi._fnProcessingDisplay( oSettings, false );

        /* Callback user function - for event handlers etc */
        if ( typeof fnCallback == 'function' && fnCallback != null )
        {
            fnCallback( oSettings );
        }
    }, oSettings );
};

$.fn.dataTableExt.oApi.fnGetAdjacentTr  = function ( oSettings, nTr, bNext )
{
    /* Find the node's position in the aoData store */
    var iCurrent = oSettings.oApi._fnNodeToDataIndex( oSettings, nTr );

    /* Convert that to a position in the display array */
    var iDisplayIndex = $.inArray( iCurrent, oSettings.aiDisplay );
    if ( iDisplayIndex == -1 )
    {
        /* Not in the current display */
        return null;
    }

    /* Move along the display array as needed */
    iDisplayIndex += (typeof bNext=='undefined' || bNext) ? 1 : -1;

    /* Check that it within bounds */
    if ( iDisplayIndex < 0 || iDisplayIndex >= oSettings.aiDisplay.length )
    {
        /* There is no next/previous element */
        return null;
    }

    /* Return the target node from the aoData store */
    return oSettings.aoData[ oSettings.aiDisplay[ iDisplayIndex ] ].nTr;
};