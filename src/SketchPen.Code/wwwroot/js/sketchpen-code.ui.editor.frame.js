var sketchPenCode = window.parent.sketchPenCode;

sketchPenCodeEditor = new function () {
    var _route, _this;
    var _editor = null;
    var _editorDecorations = null;

    this.route = () => _route;
    this.editor = () => _editor;

    this.init = function (route, value, language) {
        _route = route;

        //console.log('init editor', id, value, language);

        $('.sketchpen-code-editor-switcher').sketchPenCode_editor_switcher();
        $('.sketchpen-code-editor-settings').sketchPenCode_editor_settings_form();
        $('.sketchpen-code-editor-code-errors').sketchPenCode_editor_errors();

        if ($('#sketchpen-code-editor-code').length === 1) {

            // register a completion item provider for DLH
            // monaco.languages.registerCompletionItemProvider('razor', getDlhCompletionProvider(monaco));

            _editor = monaco.editor.create(document.getElementById('sketchpen-code-editor-code'), {
                language: language || 'text',
                automaticLayout: true,
                theme: sketchPenCode.editorTheme()
            });

            _editor.setValue(value || '');
            _editor.getModel().onDidChangeContent((event) => {
                sketchPenCodeEditor.events.fire('editor-value-changed', { route: _route, value: _editor.getValue() });

                this.setDirty();
                this.removeDecoration();
            });

        } else {
            $('.sketchpen-code-editor-settings').css('display', 'block');
        }

        if ($('.sketchpen-code-editor-settings .sketchpen-access-control').length > 0) {
            sketchPenCode.api.get('authprefixes', function (result) {
                $('.sketchpen-code-editor-settings .sketchpen-access-control').each(function (i, element) {
                    $(element).sketchpen_autocomplete_multiselect({
                        source: 'AuthAutocomplete',
                        name: 'access_string',
                        prefixes: result,
                        value: $(element).attr('data-value')
                    });
                });
            });
        }

        sketchPenCode.events.on('save-document', _event_save_document);
        sketchPenCode.events.on('verify-document', _event_verify_document);
        sketchPenCode.events.on('before-run-document', _event_before_run_document);
        sketchPenCode.events.on('destroy-editor', _event_destroy_editor);
        sketchPenCode.events.on('theme-changed', _event_theme_changed, this);

        sketchPenCode.bindDocumentEvents(window.document);

        sketchPenCode.events.fire('document-opened', { route: _route });
    };

    var _event_save_document = function (channel, args) {
        if (args.route === _route) {
            sketchPenCodeEditor.submitForm();
        }
    };

    var _event_verify_document = function (channel, args) {
        if (args.route === _route) {
            var ids = args.route.split('@')
            if (ids.length === 3) {  // view
                sketchPenCodeEditor.submitForm(true);
            }
        }
    };

    var _event_before_run_document = function (channel, args) {
        if (args.route === _route) {
            args.urlParameters = "route=" + args.route + '&globals=' + args.globals;
        }
    };

    var _event_destroy_editor = function (channel, args) {
        if (args.route === _route) {
            console.log('destroy editor ' + _route);
            sketchPenCode.events.off('save-document', _event_save_document);
            sketchPenCode.events.off('verify-document', _event_verify_document);
            sketchPenCode.events.off('before-run-document', _event_before_run_document);
            sketchPenCode.events.off('theme-changed', _event_theme_changed);
            //sketchPenCode.events.off('destroy-editor', _event_destroy_editor);

            _route = null;
        }
    }

    var _event_theme_changed = function (channel, args) {
        if (_editor) {
            _editor.updateOptions({ theme: args.theme });
        }
    };

    this.refreshToken = function (index) {
        $("input[name=sketchpen_token" + index + "]").val(this.generateRandomToken(64));
        sketchPenCodeEditor.setDirty();
    };
    this.clearToken = function (index) {
        $("input[name=sketchpen_token" + index + "]").val('');
        sketchPenCodeEditor.setDirty();
    };

    this.generateRandomToken = function (length) {
        var chars = 'abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789';

        var token = '';
        for (var i = 0; i < length; i++) {
            token += chars[parseInt((Math.random()) * 0x10000) % chars.length];
        }

        return token;
    };

    this.addErrorDecoration = function (lineNumber) {
        if (lineNumber <= 0) {
            this.removeDecoration();
            return;
        }

        _editorDecorations = sketchPenCodeEditor.editor().deltaDecorations(
            _editorDecorations || [],
            [
                {
                    range: new monaco.Range(lineNumber, 1, lineNumber, 1),
                    options: {
                        isWholeLine: true,
                        linesDecorationsClassName: 'errorLineDecoration'
                    }
                }
            ]
        );
    };
    this.removeDecoration = function () {
        if (_editorDecorations) {
            sketchPenCodeEditor.editor().deltaDecorations(_editorDecorations, []);
            _editorDecorations = null;

            sketchPenCodeEditor.events.fire('editor-decoration-removed');
        }
    }

    this.delete = function () {
        sketchPenCode.events.fire('delete-document', { route: _route });
    };

    this.setDirty = function () {
        sketchPenCode.events.fire('document-changed', { route: _route });
    };

    this.submitForm = function (verifyOnly) {
        var $form =
            $(".sketchpen-code-editor-settings")
                .children('form');

        var actionUrl = $form.attr('action');

        $.ajax({
            type: "POST",
            url: actionUrl + '?verifyOnly=' + (verifyOnly ? 'true' : 'false'),
            data: $form.serialize(), // serializes the form's elements.
            success: function (data) {
                if (data.success === true) {
                    if (verifyOnly === true) {
                        sketchPenCode.events.fire('document-verified', { route: _route });
                    } else {
                        sketchPenCode.events.fire('document-saved', { route: _route });
                    }
                } else {
                    sketchPenCode.events.fire('document-errors', { route: _route, errors: data });
                }
            }
        });
    };
}();

sketchPenCode.implementEventController(sketchPenCodeEditor);

(function ($) {
    "use strict";
    $.fn.sketchPenCode_editor_switcher = function (method) {
        if (methods[method]) {
            return methods[method].apply(this, Array.prototype.slice.call(arguments, 1));
        }
        else if (typeof method === 'object' || !method) {
            return methods.init.apply(this, arguments);
        }
        else {
            $.error('Method ' + method + ' does not exist on jQuery.sketchPenCode_editor_switcher');
        }
    };
    var defaults = {

    };
    var methods = {
        init: function (options) {
            var settings = $.extend({}, defaults, options);
            return this.each(function () {
                new initUI(this, settings);
            });
        }
    };

    var initUI = function (parent, opitons) {
        var $parent = $(parent);

        var $code = $("<div>")
            .addClass('switch-button code')
            .appendTo($parent)
            .click(function (e) {
                e.stopPropagation();
                $('.switch-to').css('display', 'none');
                $('.switch-to.code').css('display', 'block');
            });

        $("<div>")
            .addClass('switch-button settings')
            .appendTo($parent)
            .click(function (e) {
                e.stopPropagation();
                $('.switch-to').css('display', 'none');
                $('.switch-to.settings').css('display', 'block');
            });

        $code.trigger('click');
    };
})(jQuery);

(function ($) {
    "use strict";
    $.fn.sketchPenCode_editor_settings_form = function (method) {
        if (methods[method]) {
            return methods[method].apply(this, Array.prototype.slice.call(arguments, 1));
        }
        else if (typeof method === 'object' || !method) {
            return methods.init.apply(this, arguments);
        }
        else {
            $.error('Method ' + method + ' does not exist on jQuery.sketchPenCode_editor_properties_form');
        }
    };
    var defaults = {

    };
    var methods = {
        init: function (options) {
            var settings = $.extend({}, defaults, options);
            return this.each(function () {
                new initUI(this, settings);
            });
        }
    };

    var initUI = function (parent, opitons) {
        var $parent = $(parent);

        $parent.find("label")
            .addClass('sketchpen-label');
        
        $parent.find("input[type=text], textarea")
            .addClass('sketchpen-input')
            .on("keydown", function (e) {
                if (e.ctrlKey || e.which === 116) {  // Ctrl or F5 => dont set document dirty
                    return;
                }

                sketchPenCodeEditor.setDirty();
            });
        $parent.find('select')
            .addClass('sketchpen-input')
            .change(function (e) {
                sketchPenCodeEditor.setDirty();
            });

        $("<br/>").insertAfter($parent.find('.sketchpen-input,.sketchpen-label'));
    };
})(jQuery);

(function ($) {
    "use strict"
    $.fn.sketchpen_autocomplete_multiselect = function (method) {
        if (methods[method]) {
            return methods[method].apply(this, Array.prototype.slice.call(arguments, 1));
        }
        else if (typeof method === 'object' || !method) {
            return methods.init.apply(this, arguments);
        }
        else {
            $.error('Method ' + method + ' does not exist on jQuery.sketchpen_autocomplete_multiselect');
        }
    };

    var defaults = {
        source: '',
        name: '',
        prefixes: [],
        prefix_separator: '::',
        alwaysIncludeOwner: ''
    };

    var methods = {
        init: function (options) {
            var $this = $(this);
            options = $.extend({}, defaults, options);

            return this.each(function () {
                new initUI(this, options);
            });
        },
        add: function (options) {
            options = $.extend({}, defaults, options);

            var prefix = $(this).find('.sketchpen-autocomplete-multiselect-prefix').val() || '';

            if (options.value !== "*") {
                $(this).find('.sketchpen-autocomplete-multiselect-prefix').children("option").each(function (i, o) {
                    if ($(o).val() && options.value.indexOf($(o).val()) === 0)
                        prefix = $(o).val();
                });
            }

            //console.log(options.value, options.prefix_separator,  options.value.indexOf(options.prefix_separator));
            if (options.value !== "*" &&
                //options.value.indexOf(prefix) !== 0 &&
                options.value.indexOf(options.prefix_separator) < 0) {  // check if options has already a prefix
                options.value = prefix + options.value;
            }

            $(this).sketchpen_autocomplete_multiselect('remove', options);

            var displayValue = options.value;
            //if (prefix && displayValue.indexOf(prefix) === 0) {
            //    displayValue = "<strong>" + prefix + "</strong>" + displayValue.substr(prefix.length, displayValue.length - prefix.length);
            //}

            var $valContainer = $(this).find('.sketchpen-autocomplete-multiselect-value-conatiner');

            var $div = $("<div>")
                .addClass('sketchpen-autocomplete-multiselect-value-item')
                .attr('data-value', options.value)
                .text(displayValue)
                .appendTo($valContainer);



            $("<div class='close-button'>")
                .addClass('close-botton')
                .appendTo($div)
                .click(function () {
                    $(this)
                        .closest('.sketchpen-autocomplete-multiselect')
                        .sketchpen_autocomplete_multiselect('remove', {
                            value: $(this).closest('.sketchpen-autocomplete-multiselect-value-item').attr('data-value')
                    });
                });

            if (options.alwaysIncludeOwner && options.alwaysIncludeOwner !== options.value) {
                $(this).sketchpen_autocomplete_multiselect('add', { value: options.alwaysIncludeOwner, suppressEvent: true });
            } else {
                $(this).sketchpen_autocomplete_multiselect('_calc');
            }

            if (!options.suppressEvent) {
                sketchPenCodeEditor.setDirty();
            }
        },
        remove: function (options) {
            var value = options.value.replace(/\\/g, '\\\\');

            $(this)
                .find(".sketchpen-autocomplete-multiselect-value-item[data-value='" + value + "']")
                .remove();

            $(this).sketchpen_autocomplete_multiselect('_calc');

            if (!options.suppressEvent) {
                sketchPenCodeEditor.setDirty();
            }
        },
        _calc: function (options) {
            var val = '';
            $(this).find('.sketchpen-autocomplete-multiselect-value-item').each(function (i, e) {
                if (val !== '') val += ',';
                val += $(e).attr('data-value');
            });
            $(this).find('.sketchpen-autocomplete-multiselect-value').val(val);
        }
    };

    var initUI = function (elem, options) {

        var $elem = $(elem);
        $elem.addClass('sketchpen-autocomplete-multiselect');

        var $inputRow = $("<tr>").appendTo($("<table style='padding:0px;margin:0px'>").appendTo($elem));

        if (options.prefixes && options.prefixes.length > 0) {
            var $select = $("<select class='sketchpen-autocomplete-multiselect-prefix sketchpen-input' />");
            $select.appendTo($("<td style='padding:0px'>").appendTo($inputRow));

            for (var i in options.prefixes) {
                var prefix = options.prefixes[i];
                $("<option value='" + prefix + "'>" + prefix + "</option>").appendTo($select);
            }

            $("<option value=''>custom</option>").appendTo($select);
        }

        var $input = $("<input class='sketchpen-autocomplete-multiselect-input sketchpen-input' type='text' />");
        $input.appendTo($("<td style='padding:0px'>").appendTo($inputRow));
        $input.keydown(function (event) {
            if (event.keyCode === 13) {
                event.preventDefault();
                $(this).closest('.sketchpen-autocomplete-multiselect').find('.add-button').trigger('click');
                return false;
            }
        });

        var $button = $("<button>+</button>")
            .addClass('add-button')
            .appendTo($("<td style='padding:0px'>").appendTo($inputRow))
            .click(function (event) {
                event.stopPropagation();
                var $elem = $(this).closest('.sketchpen-autocomplete-multiselect');
                $elem.sketchpen_autocomplete_multiselect('add', { value: $elem.find('.sketchpen-autocomplete-multiselect-input').val(), alwaysIncludeOwner: options.alwaysIncludeOwner });
                return false;
            });

        var $valContainer = $("<div>")
            .addClass('sketchpen-autocomplete-multiselect-value-conatiner')
            .appendTo($elem);
        var $value = $("<input type='hidden' name='" + options.name + "' id='" + options.name + "' />")
            .addClass('sketchpen-autocomplete-multiselect-value')
            .appendTo($elem);

        if (options.value) {
            console.log('options.value', options.value);
            $.each(options.value.split(','), function (i, item) {
                $elem.sketchpen_autocomplete_multiselect('add', { value: item, suppressEvent: true });
            });
        }

        if ($.fn.typeahead) {
            $input.on({
                'typeahead:select': function (e, item) {
                    $(this).closest('.sketchpen-autocomplete-multiselect').sketchpen_autocomplete_multiselect('add', { value: item, alwaysIncludeOwner: options.alwaysIncludeOwner });
                },
                'keyup': function (e) {
                    if (e.keyCode === 13) {
                        $(this).typeahead('close');
                    }
                }
            })
                .typeahead({
                    hint: false,
                    highlight: false,
                    minLength: 3
                },
                    {
                        limit: Number.MAX_VALUE,
                        async: true,
                        source: function (query, processSync, processAsync) {
                            var $element = $(this.$el[0].parentElement.parentElement).children(".sketchpen-autocomplete-multiselect-input").first(); // Ugly!!!
                            var $prefix = $(this.$el[0].parentElement.parentElement).closest('.sketchpen-autocomplete-multiselect').find('.sketchpen-autocomplete-multiselect-prefix');

                            var source = $element.data('sketchpen-multiselect-source');
                            if ($prefix.length > 0) {
                                source += (source.indexOf('?') > 0 ? '&' : '?') + 'prefix=' + $prefix.val();
                            }

                            return $.ajax({
                                url: source,
                                type: 'get',
                                data: { term: query },
                                success: function (data) {
                                    //console.log(data);
                                    data = data.slice(0, 12);
                                    //console.log(data);
                                    processAsync(data);
                                },
                                error: function () {
                                }
                            });
                        }
                    }).data('sketchpen-multiselect-source', options.source);

        } else if ($.fn.autocomplete) {
            $input.autocomplete({
                search: function (event, ui) {
                    var source = $(this).data('sketchpen-multiselect-source');
                    var $prefix = $(this).closest('.sketchpen-autocomplete-multiselect').find('.sketchpen-autocomplete-multiselect-prefix');
                    if ($prefix.length > 0) {
                        source += (source.indexOf('?') > 0 ? '&' : '?') + 'prefix=' + $prefix.val();
                    }
                    $(this).autocomplete('option', 'source', source);
                },
                minLength: 3,
                select: function (event, ui) {
                    $(this).closest('.sketchpen-autocomplete-multiselect').sketchpen_autocomplete_multiselect('add', { value: ui.item.value, alwaysIncludeOwner: options.alwaysIncludeOwner });
                }
            }).data('sketchpen-multiselect-source', options.source);
        }
    };

})(jQuery);

(function ($) {
    "use strict"
    $.fn.sketchPenCode_editor_errors = function (method) {
        if (methods[method]) {
            return methods[method].apply(this, Array.prototype.slice.call(arguments, 1));
        }
        else if (typeof method === 'object' || !method) {
            return methods.init.apply(this, arguments);
        }
        else {
            $.error('Method ' + method + ' does not exist on jQuery.sketchPenCode_editor_errors');
        }
    };

    var defaults = {
    };

    var methods = {
        init: function (options) {
            var $this = $(this);
            options = $.extend({}, defaults, options);

            return this.each(function () {
                new initUI(this, options);
            });
        },
        add: function (options) {
            var $content = $(this).children('.content'), lineNumber = 0;

            var model = sketchPenCodeEditor.editor().getModel();
            //console.log(model);

            var codeLine = (options.code_line || '').trim();
            if (codeLine.indexOf("Write(") == 0) {
                codeLine = codeLine.substr(6);
            }
            if (codeLine.indexOf(");") == codeLine.length - 2) {
                codeLine = codeLine.substr(0, codeLine.length - 2);
            }
            //console.log(codeLine);

            if (codeLine) {
                for (var i = 1, to = model.getLineCount(); i <= to; i++) {
                    var line = model.getLineContent(i);

                    if (!line.trim())
                        continue;

                    //console.log('******' + line);

                    if (line.indexOf(codeLine) >= 0) {
                        lineNumber = i;
                        break;
                    }
                }
            }

            console.log(lineNumber);
            
            $("<div>")
                .addClass(options.is_warning === true ? 'warning' : 'error')
                .text('Line ' + lineNumber + ': ' + options.error_text)
                .data('line', lineNumber)
                .appendTo($content)
                .click(function (e) {
                    e.stopPropagation();

                    var lineNumber = $(this).data('line');
                    //console.log('select line ' + lineNumber);

                    $(this).parent().children('.selected').removeClass('selected');
                    $(this).addClass('selected');

                    sketchPenCodeEditor.addErrorDecoration(lineNumber);
                })
        }
    };

    var initUI = function (elem, options) {
        var $elem = $(elem);

        var $title = $("<div><span class='title'></span></<div>")
            .addClass('titlebar')
            .appendTo($elem);

        $("<div>")
            .addClass('closebutton')
            .appendTo($title)
            .click(function (e) {
                e.stopPropagation();
                $(this).closest('.sketchpen-code-editor-code-panel').removeClass('show-errors');
            });

        var $content = $("<div>")
            .addClass('content')
            .appendTo($elem);

        sketchPenCode.events.on(['document-saved', 'document-verified'], _event_document_saved, $elem);
        sketchPenCode.events.on('document-errors', _event_document_errors, $elem);
        sketchPenCode.events.on('destroy-editor', _event_destroy_editor);

        sketchPenCodeEditor.events.on('editor-decoration-removed', function (channel) {
            $content.children('.selected').removeClass('selected');
        });
    };

    var _event_document_saved = function (channel, args) {
        if (args.route === sketchPenCodeEditor.route()) {
            var $content = this.children('.content');

            $content.empty();
            $content.closest('.sketchpen-code-editor-code-panel').removeClass('show-errors');
        }
    };

    var _event_document_errors = function (channel, args) {
        if (args.route === sketchPenCodeEditor.route()) {
            var $elem = this;
            console.log($elem);

            var $content = $elem.children('.content');
            var $title = $elem.children('.titlebar');

            $content.empty();
            $content.closest('.sketchpen-code-editor-code-panel').addClass('show-errors');

            console.log(args);

            var errors = args.errors;
            console.log(errors);
            if (errors) {
                $title.children('.title').text(errors.error_message);

                $.each(errors.compiler_errors, function (i, e) {
                    $elem.sketchPenCode_editor_errors('add', e);
                });
            }
        }
    };

    var _event_destroy_editor = function (channel, args) {
        if (args.route === sketchPenCodeEditor.route()) {
            console.log('destroy sketchPenCode_editor_errors');
            sketchPenCode.events.off('document-saved', _event_document_saved);
            sketchPenCode.events.off('document-errors', _event_document_errors);
            //sketchPenCode.events.off('destroy-editor', _event_destroy_editor);
        }
    };

})(jQuery);