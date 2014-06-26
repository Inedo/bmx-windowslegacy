function BmAppPoolSelector(o) {
    /// <param name="o" value="{
    /// inputSelector:'',
    /// getAppPoolsUrl:''
    /// }"/>

    $(o.inputSelector).select2({
        ajax: {
            url: o.getAppPoolsUrl,
            dataType: 'json',
            data: function (term) {
                var serverId = $('#bm-action-server-id').val();
                if (!/^[0-9]+$/.test(serverId))
                    serverId = 0;
                else
                    serverId = parseInt(serverId);

                return {
                    serverId: serverId,
                    term: term
                };
            },
            results: function (data) {
                return {
                    results: $.map(data, function (s) { return { id: s, text: s } })
                };
            }
        },
        createSearchChoice: function (term) {
            return {
                id: term,
                text: term
            };
        },
        initSelection: function (element, callback) {
            var value = $(element).val();
            if (value) {
                callback({
                    id: value,
                    text: value
                });
            }
        }
    });
}