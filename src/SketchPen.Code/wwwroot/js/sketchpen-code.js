var sketchPenCode = new function ($) {
    "use strict"
    let _targetUrl, _id;
    let _editorTheme = 'vs-dark';
    let _userPrivileges;

    let $tree, $globals, $editor, $toolbar;

    this.targetUrl = () => _targetUrl;
    this.id = () => _id;
    this.editorTheme = () => _editorTheme;
    this.loginUsername = () => "";

    this.fileExt = () => ".sp";
    this.templateFileExt = ()=> ".spt";
    this.globalsFileExt = () => ".globals";
    this.fileType = (file) => {
        if (file.indexOf(this.fileExt()) == file.length - this.fileExt().length) {
            return "Sketchpen File";
        }
        if (file.indexOf(this.templateFileExt()) == file.length - this.templateFileExt().length) {
            return "Sketchpen Template File";
        }
        if (file.indexOf(this.globalsFileExt()) === file.length - this.globalsFileExt().length) {
            return "Sketchpen Globals File";
        }

        return "Unknown Filetype";
    };
    this.fileClass = (file) => {
        if (file.indexOf(this.fileExt()) == file.length - this.fileExt().length) {
            return "sp-file";
        }
        if (file.indexOf(this.templateFileExt()) == file.length - this.templateFileExt().length) {
            return "template-file";
        }
        if (file.indexOf(this.globalsFileExt()) === file.length - this.globalsFileExt().length) {
            return "globals-file";
        }

        return "folder";
    }

    this.start = function (targetUrl, id) {
        _targetUrl = targetUrl;
        _id = id;
        _userPrivileges = { createFiles: true, deleteFiles: true };

        $tree = $('.sketchpen-code-tree-container').sketchPenCode_tree({
            $toolbar: $('.sketchpen-code-tree-top > .sketchpen-code-tree-toolbar')
        });
        $globals = $('.sketchpen-code-globlals-container').sketchPenCode_globals();
        $editor = $('.sketchpen-code-content').sketchPenCode_editor();
        $toolbar = $('.sketchpen-code-toolbar').sketchPenCode_toolbar();

        $editor.sketchPenCode_editor('addTab', { title: 'Start', route: '_start', className: 'start', hideCloseButton: true });

        this.bindDocumentEvents(window.document);

        sketchPenCode.events.on('refresh-ui', function (channel) {
            var args = {
                currentDoc: $editor.sketchPenCode_editor('currentDoc'),
                dirtyDocs: $editor.sketchPenCode_editor('dirtyDocs')
            };

            sketchPenCode.events.fire('refresh-ui-elements', args);
        });

        sketchPenCode.events.on('save-current-document', function () {
            let route = $editor.sketchPenCode_editor('currentDoc');
            if (route) {
                sketchPenCode.events.fire('save-document', { route: route });
            }
        });

        sketchPenCode.events.on('verify-current-document', function () {
            let route = $editor.sketchPenCode_editor('currentDoc');
            if (route) {
                sketchPenCode.events.fire('verify-document', { route: route });
            }
        });

        sketchPenCode.events.on('save-all-documents', function () {
            let routes = $editor.sketchPenCode_editor('dirtyDocs');
            $.each(routes, function (i, route) {
                sketchPenCode.events.fire('save-document', { route: route });
            });
        });

        sketchPenCode.events.on('delete-document', function (channel, args) {
            let file = args.file;

            let fireDeleted = function (result) {
                if (result.success) {
                    sketchPenCode.events.fire('document-deleted', result);
                } else {
                    sketchPenCode.ui.alert("Error", (result.error_message || 'Unknown error'));
                }
            }

            sketchPenCode.ui.confirm('Delete ' + sketchPenCode.fileType(file), 'Delete ' + file + ' permanently?',
                function () {
                    sketchPenCode.api.deleteFile(file, function (result) {
                        fireDeleted(result);
                    });
                });
        });

        sketchPenCode.events.on(['run-current-document-in-tab', 'run-current-document'], function (channel) {
            let route = $editor.sketchPenCode_editor('currentDoc');

            let cmd =  '/preview';
            let url = sketchPenCode.targetUrl() + cmd;

            let args = { route: route, globals: $globals.sketchPenCode_globals('currentValue') }
            sketchPenCode.events.fire('before-run-document', args);  // collect url parameters
            if (args.urlParameters) {
                url += '?' + args.urlParameters;
            }
            //console.log(channel, url);

            if (channel.channel === 'run-current-document-in-tab') {
                window.open(url);
            } else {
                $('body').sketchPenCode_blockframe({
                    onShow: function ($content) {
                        $("<iframe>")
                            .attr('src', url)
                            .css({
                                position: 'absolute',
                                left: 0, right: 0, top: 0, bottom: 0,
                                width: '100%', height: '100%',
                                border: 'none'
                            })
                            .appendTo($content);
                    }
                });
            }
        });

        sketchPenCode.events.on('toggle-color-scheme', function (channel) {
            $('.sketchpen-code-ide').toggleClass('colorscheme-light');
            _editorTheme = $('.sketchpen-code-ide').hasClass('colorscheme-light') ? 'vs' : 'vs-dark';

            sketchPenCode.events.fire('theme-changed',
                {
                    theme: _editorTheme
                });
        }, this);

        sketchPenCode.events.on('toggle-help', function (channel) {
            let $sketchpenBody = $('.sketchpen-code-body');
            $sketchpenBody.toggleClass('showhelp');

            if ($sketchpenBody.hasClass('showhelp')) {
                $sketchpenBody.find('.sketchpen-code-help > #help-frame').attr('src', _sketchPenEngineUrl + '/help');
            }
        });

        sketchPenCode.events.on('logout', function (channel) {
            document.location = document.location + '/logout';
        });

        $('.sketchpen-code-tree-collapse-button').click(function (e) {
            e.stopPropagation();
            $(this).closest('.sketchpen-code-ide').toggleClass('tree-collapsed');
        });

        $(window).resize(function () {
            sketchPenCode.events.fire('ide-resize');
        });

        sketchPenCode.events.fire('refresh-ui');
    };

    this.implementEventController = function (obj) {
        obj.events = new sketchPenCode.eventController(obj);
    };

    this.api = new function () {
        this.get = function (route, callback, data) {
            $.ajax({
                url: sketchPenCode.targetUrl() + '/' + route + '/' + sketchPenCode.id(),
                data: data,
                success: function (result) {
                    callback(result)
                }
            });
        };

        this.getGlobals = function (callback) {
            this.get('getGlobals', callback);
        }

        this.getFiles = function (callback) {
            this.get('getFiles', callback)
        };

        this.createFile = function (filename, callback) {
            this.get('createFile', callback, { filename: filename });
        };

        this.deleteFile = function (filename, callback) {
            this.get('deleteFile', callback, { filename: filename });
        };

        this.getComposers = function (callback) {
            console.log(sketchPenCode.targetUrl() + '/getComposers');
            $.ajax({
                url: sketchPenCode.targetUrl() + '/getComposers',
                success: function (result) {
                    callback(result)
                }
            });
        };
    };

    this.privileges = new function () {
        this.createFiles = function () { return _userPrivileges.createFiles === true; };
        this.deleteFiles = function () { return _userPrivileges.deleteFiles === true; };
    };

    this.timer = function (callback, duration, arg) {
        let _timer = 0;
        let _callback = callback;
        let _duration = duration;
        let _arg = arg;
        this.SetArgument = function (arg) { _arg = arg; };
        this.SetDuration = function (d) {
            _duration = d;
        };
        this.Duration = function () { return _duration; };
        this.Start = function (arg) {
            window.clearTimeout(_timer);
            if (arg)
                _arg = arg;
            if (_duration == 0) {
                if (_arg)
                    _callback(_arg);
                else
                    _callback();
            }
            else {
                if (_arg)
                    _timer = window.setTimeout(function () { _callback(_arg); }, _duration);
                else
                    _timer = window.setTimeout(_callback, _duration);
            }
        };
        this.StartWith = function (callbackFunction) {
            _callback = callbackFunction;
            this.Start();
        };
        this.Stop = function () { window.clearTimeout(_timer); };
        this.Exec = function () {
            window.clearTimeout(_timer);
            if (_arg)
                _callback(_arg);
            else
                _callback();
        };
        this.start = this.Start;
        this.stop = this.Stop;
        this.startWidth = this.StartWidth;
        this.exec = this.Exec;
    };

    this.delayed = function (callback, duration, arg) {
        let timer = new sketchPenCode.timer(callback, duration ? duration : 1, arg);
        timer.Start();
    };

    this.allDocuments = function () {
        let documents = [];

        $tree.find('.tree-node').each(function (i, node) {
            let $node = $(node);
            let route = $node.data('data-route');
            if (!$node.hasClass('add') && route) {
                documents.push({
                    id: route,
                    isOpen: $editor.sketchPenCode_editor('isOpen', { id: route })
                });
            }
        });

        return documents;
    }

    this.bindDocumentEvents = function (doc) {
        $(doc).bind("keyup keydown", function (e) {
            if (e.ctrlKey && e.shiftKey && e.which == 83) { // Ctrl + Shift + s
                if (e.type == 'keyup') {
                    e.stopPropagation();
                    sketchPenCode.events.fire('save-all-documents');
                }
                return false;
            }
            if (e.ctrlKey && e.which == 83) {  // Ctrl + s
                if (e.type == 'keyup') {
                    e.stopPropagation();
                    sketchPenCode.events.fire('save-current-document');
                }
                return false;
            }

            if (e.which === 116) { // F5 ( Ctrl + F5 )
                if (e.type == 'keyup') {
                    e.stopPropagation();
                    sketchPenCode.events.fire(e.ctrlKey ? 'run-current-document-in-tab' : 'run-current-document');
                }
                return false;
            }

            if (e.key === "Escape") {
                $('body').sketchPenCode_blockframe('close');
                return false;
            }
        });
    };

    this.ui = new function () {
        this.alert = function (title, message) {
            $('body').sketchPen_code_modal({
                title: title,
                height: '200px',
                id: 'sketchpen-code-alert',
                onload: function ($content) {
                    $("<p>")
                        .text(message)
                        .appendTo($content.addClass('sketchpen-code-messagebox-content'));

                    let $buttonbar = $("<div>").addClass("button-bar").appendTo($content);

                    $("<button>")
                        .addClass("sketchpen-code-button")
                        .text("OK")
                        .appendTo($buttonbar)
                        .click(function () {
                            $('body').sketchPen_code_modal('close', { id: 'sketchpen-code-alert' });
                        });
                }
            });
        };

        this.confirm = function (title, message, onConfirm) {
            sketchPenCode.ui.confirmIf(true, title, message, onConfirm);
        }

        this.confirmIf = function (contition, title, message, onConfirm) {
            if (contition === false) {
                if (onConfirm) {
                    onConfirm();
                }
                return;
            }

            $('body').sketchPen_code_modal({
                title: title,
                height: '200px',
                id: 'sketchpen-code-confirm',
                onload: function ($content) {
                    $("<p>")
                        .text(message)
                        .appendTo($content.addClass('sketchpen-code-messagebox-content'));

                    let $buttonbar = $("<div>").addClass("button-bar").appendTo($content);

                    $("<button>")
                        .addClass("sketchpen-code-button cancel")
                        .text("No")
                        .appendTo($buttonbar)
                        .click(function () {
                            $('body').sketchPen_code_modal('close', { id: 'sketchpen-code-confirm' });
                        });

                    $("<button>")
                        .addClass("sketchpen-code-button")
                        .text("Yes")
                        .appendTo($buttonbar)
                        .click(function () {
                            if (onConfirm) {
                                onConfirm();
                            }

                            $('body').sketchPen_code_modal('close', { id: 'sketchpen-code-confirm' });
                        });
                }
            });
        }

        this.prompt = function (title, message, onEntered, placeholder) {
            $('body').sketchPen_code_modal({
                title: title,
                height: '200px',
                id: 'sketchpen-code-prompt',
                onload: function ($content) {
                    $("<p>")
                        .text(message)
                        .appendTo($content.addClass('sketchpen-code-messagebox-content'));

                    let $input = $("<input>")
                        .addClass('input-prompt')
                        .attr('type', 'text')
                        .attr('placeholder', placeholder || '')
                        .appendTo($content)
                        .keyup(function (e) {
                            $msg.css('display', $(this).val().trim() ? 'none' : '');
                        });

                    let $msg = $("<p>")
                        .css({ display: 'none', color: 'red' })
                        .text('Please, insert a value')
                        .appendTo($content);

                    let $buttonbar = $("<div>").addClass("button-bar").appendTo($content);

                    $("<button>")
                        .addClass("sketchpen-code-button cancel")
                        .text("Cancel")
                        .appendTo($buttonbar)
                        .click(function () {
                            $('body').sketchPen_code_modal('close', { id: 'sketchpen-code-prompt' });
                        });

                    $("<button>")
                        .addClass("sketchpen-code-button")
                        .text("OK")
                        .appendTo($buttonbar)
                        .click(function () {
                            var val = $input.val().trim();

                            if (!val) {
                                $msg.css('display', '');
                                return;
                            }

                            if (onEntered) {
                                onEntered(val);
                            }

                            $('body').sketchPen_code_modal('close', { id: 'sketchpen-code-prompt' });
                        });
                }
            });
        };
    };
}(jQuery);