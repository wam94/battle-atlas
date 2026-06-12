from terrain_pipeline import cli


def test_parser_has_fetch_and_build():
    p = cli.make_parser()
    assert p.parse_args(["fetch"]).command == "fetch"
    args = p.parse_args(["build"])
    assert args.command == "build"
