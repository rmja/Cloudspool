export default angular.module('directives', [])
    .directive('bootstrapFileinput', function() {
        return {
            restrict: 'A',
            link: function(scope, element: any, attrs) {

                // https://github.com/kartik-v/bootstrap-fileinput/issues/199

                var options: any = {
                    showPreview: false,
                    showUpload: false,
                    showRemove: false,
                    layoutTemplates: {
                        icon: '<i class="fa fa-file"></i> &nbsp;'
                    },
                    browseIcon: '<i class="fa fa-folder-open"></i> ',
                    browseClass: 'btn btn-default'
                };

                switch (attrs.bootstrapFileinput) {
                    case 'select':
                        options.showCaption = true;
                        options.browseLabel = 'Select';
                        break;
                    case 'open':
                        options.showCaption = false;
                        options.browseLabel = 'Open';
                        break;
                }
                element.fileinput(options);
            }
        }
    })
    .name;