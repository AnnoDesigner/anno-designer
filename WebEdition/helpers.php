<?

function tryGET($index, &$refValue) {
	if (!isset($_GET[$index])) {
		return false;
	}
	$refValue = $_GET[$index];
	return true;
}

function get_int($var) {
    if (str_is_int($var)) {
        return intval($var);
    }
    return null;
}

function str_is_int($var) {
    return preg_match('/[0-9]+/', $var) == 1;
}

?>