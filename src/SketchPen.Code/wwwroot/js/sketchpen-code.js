var sketchPenCode = new function ($) {
    var _targetUrl, _sketchPenEngineUrl, _username, _userPrevileges;
    var _editorTheme = 'vs-dark';

    var $tree, $editor, $toolbar;

    this.targetUrl = () => _targetUrl;
    this.loginUsername = () => _username;
    this.editorTheme = () => _editorTheme;

    this.start = function (targetUrl, sketchPenEngineUrl, username, userPrivileges) {
        _targetUrl = targetUrl;
        _sketchPenEngineUrl = sketchPenEngineUrl;
        _username = username;
        _userPrevileges = userPrivileges || {};

        $tree = $('.sketchpen-code-tree-container').sketchPenCode_tree({
            $toolbar: $('.sketchpen-code-tree-top > .sketchpen-code-tree-toolbar')
        });
        $editor = $('.sketchpen-code-content').sketchPenCode_editor();
        $toolbar = $('.sketchpen-code-toolbar').sketchPenCode_toolbar();

        $editor.sketchPenCode_editor('addTab', { title: 'Start', id: '_start', className: 'start', hideCloseButton: true });

        this.bindDocumentEvents(window.document);

        sketchPenCode.events.on('refresh-ui', function (channel, args) {
            var args = {
                currentDoc: $editor.sketchPenCode_editor('currentDoc'),
                dirtyDocs: $editor.sketchPenCode_editor('dirtyDocs')
            };

            sketchPenCode.events.fire('refresh-ui-elements', args);
        });

        sketchPenCode.events.on('save-current-document', function () {
            var id = $editor.sketchPenCode_editor('currentDoc');
            if (id) {
                sketchPenCode.events.fire('save-document', { id: id });
            }
        });

        sketchPenCode.events.on('verify-current-document', function () {
            var id = $editor.sketchPenCode_editor('currentDoc');
            console.log('verify-current-document',id)
            if (id) {
                sketchPenCode.events.fire('verify-document', { id: id });
            }
        });

        sketchPenCode.events.on('save-all-documents', function () {
            var ids = $editor.sketchPenCode_editor('dirtyDocs');
            $.each(ids, function (i, id) {
                sketchPenCode.events.fire('save-document', { id: id });
            });
        });

        sketchPenCode.events.on('delete-document', function (channel, args) {
            var ids = args.id.split('@');

            var fireDeleted = function (result, args) {
                if (result.success) {
                    sketchPenCode.events.fire('document-deleted', args);
                } else {
                    sketchPenCode.ui.alert("Error", (result.error_message || 'Unknown error'));
                }
            }

            if (ids.length === 1) {
                sketchPenCode.ui.confirm('Delete endpoint', 'Delete endpoint ' + args.id + ' permanently?',
                    function () {
                        sketchPenCode.api.deleteEndPoint(ids[0], function (result) {
                            fireDeleted(result, args);
                        });
                    });
            }
            else if (ids.length === 2) {
                sketchPenCode.ui.confirm('Delete query', 'Delete query ' + args.id + ' permanently?',
                    function () {
                        sketchPenCode.api.deleteQuery(ids[0], ids[1], function (result) {
                            fireDeleted(result, args);
                        });
                    });
            }
            else if (ids.length === 3) {
                sketchPenCode.ui.confirm('Delete view', 'Delete view ' + args.id + ' permanently?',
                    function () {
                        sketchPenCode.api.deleteView(ids[0], ids[1], ids[2], function (result) {
                            fireDeleted(result, args);
                        });
                    });
            }
        });

        sketchPenCode.events.on(['run-current-document-in-tab', 'run-current-document'], function (channel) {
            var id = $editor.sketchPenCode_editor('currentDoc');

            var cmd = id.split('@').length === 3 ? '/report/' : '/select/';
            var url = _sketchPenEngineUrl + cmd + id;

            var args = { id: id, urlParameters: '' }
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
            var $sketchpenBody = $('.sketchpen-code-body');
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
                url: sketchPenCode.targetUrl() + '/' + route,
                data: data,
                success: function (result) {
                    callback(result)
                }
            });
        };

        this.getEndPoints = function (callback) {
            this.get('getEndPoints', callback)
        };
        this.getQueries = function (endPoint, callback) {
            this.get('getQueries?endPoint=' + endPoint, callback);
        };
        this.getViews = function (endPoint, query, callback) {
            this.get('getViews?endPoint=' + endPoint + '&query=' + query, callback);
        };

        this.createEndPoint = function (endPoint, callback) {
            this.get('createEndPoint', callback, { endPoint: endPoint });
        };
        this.createQuery = function (endPoint, query, callback) {
            this.get('createEndPointQuery', callback, { endPoint: endPoint, query: query });
        };
        this.createView = function (endPoint, query, view, callback) {
            this.get('createEndPointQueryView', callback, { endPoint: endPoint, query: query, view: view });
        };

        this.deleteEndPoint = function (endPoint, callback) {
            this.get('deleteEndPoint', callback, { endPoint: endPoint });
        };
        this.deleteQuery = function (endPoint, query, callback) {
            this.get('deleteEndPointQuery', callback, { endPoint: endPoint, query: query });
        };
        this.deleteView = function (endPoint, query, view, callback) {
            this.get('deleteEndPointQueryView', callback, { endPoint: endPoint, query: query, view: view });
        };

        this.verifyView = function (endPoint, query, view, callback) {
            this.get('verifyEndPointQueryView', callback, { endPoint: endPoint, query: query, view: view });
        };
    };

    this.privileges = new function () {
        this.createEndpoints = function () { return _userPrevileges.createEndpoints === true; };
        this.createQueries = function () { return _userPrevileges.createQueries === true; };
        this.createViews = function () { return _userPrevileges.createViews === true; };
        this.deleteEndpoints = function () { return _userPrevileges.deleteEndpoints === true; };
        this.deleteQueries = function () { return _userPrevileges.deleteQueries === true; };
        this.deleteViews = function () { return _userPrevileges.deleteViews === true; };
    };

    this.timer = function (callback, duration, arg) {
        var _timer = 0;
        var _callback = callback;
        var _duration = duration;
        var _arg = arg;
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
        var timer = new sketchPenCode.timer(callback, duration ? duration : 1, arg);
        timer.Start();
    };

    this.allDocuments = function () {
        var documents = [];

        $tree.find('.tree-node').each(function (i, node) {
            var $node = $(node);
            var route = $node.data('data-route');
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

                    var $buttonbar = $("<div>").addClass("button-bar").appendTo($content);

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
                id: 'sketchpen-code-alert',
                onload: function ($content) {
                    $("<p>")
                        .text(message)
                        .appendTo($content.addClass('sketchpen-code-messagebox-content'));

                    var $buttonbar = $("<div>").addClass("button-bar").appendTo($content);

                    $("<button>")
                        .addClass("sketchpen-code-button cancel")
                        .text("No")
                        .appendTo($buttonbar)
                        .click(function () {
                            $('body').sketchPen_code_modal('close', { id: 'sketchpen-code-alert' });
                        });

                    $("<button>")
                        .addClass("sketchpen-code-button")
                        .text("Yes")
                        .appendTo($buttonbar)
                        .click(function () {
                            if (onConfirm) {
                                onConfirm();
                            }

                            $('body').sketchPen_code_modal('close', { id: 'sketchpen-code-alert' });
                        });
                }
            });
        }
     }
}(jQuery);