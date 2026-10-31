(function ($) {
    "use strict";

    function enableAutocomplete(selector, url, placeholder) {
        var $select = $(selector);
        if (!$select.length || typeof $.fn.select2 !== "function") return;

        var selectedValue = $select.val();
        var selectedText = selectedValue
            ? $select.find("option:selected").text()
            : null;

        // Keep only the selected value; all other choices arrive via AJAX.
        $select.empty().append(new Option("", "", !selectedValue, !selectedValue));
        if (selectedValue && selectedValue !== "0" && selectedText) {
            $select.append(new Option(selectedText, selectedValue, true, true));
        }

        $select.select2({
            width: "100%",
            placeholder: placeholder,
            allowClear: true,
            minimumInputLength: 3,
            language: {
                inputTooShort: function (args) {
                    return "Enter at least " + args.minimum + " characters";
                },
                noResults: function () { return "No matches found"; },
                searching: function () { return "Searching..."; }
            },
            ajax: {
                url: url,
                dataType: "json",
                delay: 300,
                cache: true,
                data: function (params) {
                    return { term: params.term || "" };
                },
                processResults: function (data) {
                    return { results: data };
                }
            }
        });
    }

    $(function () {
        enableAutocomplete("#CategoryId", "/Products/Autocomplete/Categories", "Search for a category...");
        enableAutocomplete("#StoreId", "/Products/Autocomplete/Stores", "Search for a store...");
    });
})(jQuery);