function BmServiceSelector(o) {
    /// <param name="o" value="{
    /// inputSelector:'',
    /// getServicesUrl:''
    /// }"/>

    $(o.inputSelector).select2({
        minimumInputLength: 1,
        ajax: {
            url: o.getServicesUrl,
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
                    results: $.map(data, function (s) { return { id: s.id, text: s.name ? (s.name + ' (' + s.id + ')') : s.id } })
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