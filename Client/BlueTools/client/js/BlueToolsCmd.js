// Created by Robert Curlette, www.curlette.com
Type.registerNamespace("Extensions");

Extensions.BlueCopy = function Extensions$BlueCopy()
{
    Type.enableInterface(this, "BlueTools.Interface");
	this.addInterface("Tridion.Cme.Command", ["BlueCopy"]);
};

Extensions.BlueCopy.prototype.isAvailable = function BlueCopy$isAvailable(selection, pipeline) {
    // Only show option for versioned items
    var items = selection.getItems();
    if (items.length > 1)
        return false;

    if (items.length == 1) {
        var item = $models.getItem(selection.getItem(0));
        if (item.getItemType() == $const.ItemType.STRUCTURE_GROUP || item.getItemType() == $const.ItemType.FOLDER || item.getItemType() == $const.ItemType.PUBLICATION) {
            return false;
        }
        return true;
    }
};

Extensions.BlueCopy.prototype.isEnabled = function BlueCopy$isEnabled(selection, pipeline) {
    var items = selection.getItems();
    if (items.length == 1) {        
            return true;
    }
    else {
        return false;
    }
};

Extensions.BlueCopy.prototype._execute = function BlueCopy$_execute(selection, pipeline) {
    // Comment line below once client is working and you want to test the server
    //alert('Really Excellent!');

    // UnComment - Show Popup that calls Web Service using AJAX
	var selectedID = selection.getItem(0);
    var host = window.location.protocol + "//" + window.location.host;
    var url = host + '/WebUI/Editors/BlueTools/client/html/popup.htm?Uri=' + selectedID;
    var popup = $popup.create(url, "toolbar=no,width=400px,height=647px,resizable=false,scrollbars=false", null);
    popup.open();
};