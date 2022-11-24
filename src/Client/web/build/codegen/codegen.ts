import $RefParser from 'json-schema-ref-parser';
import { NgOpenApiGen } from 'ng-openapi-gen';
import { Options } from 'ng-openapi-gen/lib/options';

const options: Options = {
  input: 'build/api.json',
  output: 'src/app/common/services/api',
  templates: 'build/codegen/templates'
};

const run = async () => {
  // load the openapi-spec and resolve all $refs
  const RefParser = new $RefParser();
  const openApi = await RefParser.bundle(options.input, {
    dereference: { circular: false }
  });

  const ngOpenGen = new NgOpenApiGen(openApi, options);
  ngOpenGen.generate();
};

run();
